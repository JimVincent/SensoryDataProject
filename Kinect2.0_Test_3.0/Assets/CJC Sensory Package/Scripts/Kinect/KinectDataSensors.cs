using UnityEngine;
using System.Collections;
using Windows.Kinect;

public enum BreathState {In, Out};

[RequireComponent(typeof(KinectManager_Body))]
public class KinectDataSensors : MonoBehaviour 
{
	public static KinectDataSensors inst;
	
	/// <summary>
	/// The window length that data is accumulated  before averaging
	/// </summary>
	[Range(0.1f, 2.0f)]public float sampleTime = 0.3f;
	
	/// <summary>
	/// How sensitive the system is to movement.
	/// eg. 100 is full sensitivity which may pick up on digital jittering input.
	/// </summary>
	[Range(0.0f, 1.0f)] public float sensitivity = 90;
	
	/// <summary>
	/// Shortest duration a breath cycle must be to be used for breathLength.
	/// eg. 0.1 seconds may result in rapid in/out breathState changes which will result in a inaccurate reading.
	/// </summary>
	[Range(0.1f, 2.0f)] public float breathStateChangeBuf = 1.0f;

	/// <summary>
	/// Threshold on leaning value that determins if it will be registered.
	/// eg. Threshold of 0.1 means anything lower than 0.1 will return 0 from the getter.
	/// </summary>
	[Range(0.0f, 1.0f)] public float leanThreshold = 0.10f;

	/// <summary>
	/// Multiplies the leaning value to make it more sensitive.
	/// eg. 8 means you have to lean 8 times less than the default.
	/// </summary>
	[Range(0.0f, 100.0f)]   public float leanSensitivty = 8.0f;
	
	/// <summary>
	/// Accessor for KinectManager static Instance
	/// </summary>
	private KinectManager_Body kinectMan;
	
	/// <summary>
	/// The current in/out breathState of the breath
	/// </summary>
	private BreathState breathState = BreathState.In;
	
	/// <summary>
	/// The reading provided from the previous sample window calculation
	/// *Note: Holds a value greater than zero after second sample window has been calculated.
	/// </summary>
	private float previousReading = 0.0f;
	
	/// <summary>
	/// The most recent sample window calculation
	/// </summary>
	private float currentReading = 0.0f;
	
	/// <summary>
	/// The duration of the most recent breath cycle.
	/// *Note: This is measured from in breath until the next in breath - 
	/// But only if value is greater than breathStateChangeBuf.
	/// </summary>
	private float breathLength = 0.0f;
	
	/// <summary>
	/// Indicates whether a body has been found and is being tracked by the kinect.
	/// *Note: On Kinect start up there is a few seconds before it tracks the users body - 
	/// Also if the user moves out of sight this will result in false.
	/// </summary>
	private bool bodyTracked = false;
	
	/// <summary>
	/// Accumulates Y pos from both shoulders over the duration of the sample window.
	/// Simple 2 var system instead of using List (performance)
	/// Both variables are reset to zero once sample window calculations are complete.
	/// </summary>
	private float logY = 0.0f; 	// acumulates Y pos during actice reading
	private int logYCount = 0; 	// counts log to determine average
	
	/// <summary>
	/// Accumulates breathLength values.
	/// Simple 2 var system instead of using List (performance)
	/// Only gets reset when user calls RefreshBreath()
	/// </summary>
	private float logLength = 0.0f;
	private int logLengthCount = 0;
	
	/// <summary>
	/// Timers for tracking durations
	/// </summary>
	private float sampleT = 0.0f;
	private float lengthT = 0.0f;
	
	/// <summary>
	/// Switch for breath cycle indication
	/// </summary>
	private bool inhaled = false;
	private bool exhaled = false;
	
	private float currentLean;
	
	
	
	// User Get Functions ////////////////////////////////////////////////////////////////////////////////////////////
	
	/// <summary>
	/// Returns true if the class is ready to produce data.
	/// </summary>
	/// <returns><c>true</c>, if is ready was gotten, <c>false</c> otherwise.</returns>
	public bool GetIsTracking()
	{
		return bodyTracked;
	}
	
	/// <summary>
	/// Gets the state of the breath as an enum.
	/// </summary>
	/// <returns>The breath state.</returns>
	public BreathState GetBreathState()
	{
		return breathState;
	}
	
	/// <summary>
	/// Gets the length of the most recent breath cycle (in and out).
	/// </summary>
	/// <returns>The breath length.</returns>
	public float GetBreathLength()
	{
		return breathLength;
	}
	
	/// <summary>
	/// Gets the average length of the breath.
	/// </summary>
	/// <returns>The average breath length.</returns>
	public float GetAverageBreathLength()
	{
		if(logLengthCount > 0)
			return logLength / logLengthCount;
		else
			return 0.0f;
	}
	
	/// <summary>
	/// Resets the breath length average and breath cycle count
	/// </summary>
	public void RefreshBreath()
	{
		logLength = logLengthCount = 0;
	}
	
	/// <summary>
	/// Gets the total breath cycles.
	/// </summary>
	/// <returns>The total breath cycles.</returns>
	public int GetTotalBreathCycles()
	{
		return logLengthCount;
	}
	
	
	// IT'S MOTHER FUCKING DONE JUM - SHIP IT
	public float GetLean()
	{
		if (Mathf.Abs (currentLean) <= leanThreshold)
			return 0;
		else
			return Mathf.Round(currentLean * leanSensitivty * 100.0f) / 100.0f;
	}
	
	// Private Functions ////////////////////////////////////////////////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes the static instance of this class
	/// </summary>
	void Awake()
	{
		if (inst == null)
			inst = this;
	}
	
	// Use this for initialization
	void Start () 
	{
		// assign static class vars
		kinectMan = KinectManager_Body.inst;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// inc timers
		sampleT += Time.deltaTime;
		
		// sample window has expired
		if(sampleT >= sampleTime)
		{
			// calc average and update readings
			previousReading = currentReading;
			currentReading = logY / logYCount;
			UpdateBreath();
			
			// reset logs / timer
			sampleT = logY = logYCount = 0;
		}
		else
		{
			// retrieve tracked body
			Body TempBody = kinectMan.GetBody();
			
			if(TempBody != null)
			{
				bodyTracked = true;
				
				// read and log shoulder data
				float leftShoulder = TempBody.Joints[JointType.ShoulderLeft].Position.Y;
				float rightShoulder = TempBody.Joints[JointType.ShoulderRight].Position.Y;
				logY += leftShoulder + rightShoulder;
				logYCount++;
				
				currentLean = TempBody.Lean.X;
				print (currentLean);
				print ("Function: " + GetLean());
				
				// measure breath duration
				if(inhaled)
					lengthT += Time.deltaTime;
				
				// Start of breath cycle
				if(!inhaled && breathState == BreathState.In)
				{
					inhaled = true;
					logLengthCount++;
				}
				// End of cycle
				else if(exhaled && breathState == BreathState.In)
				{
					// log the breath duration
					breathLength = lengthT;
					logLength += lengthT;
					
					// reset timer / inhale switch
					lengthT = 0.0f;
					inhaled = exhaled = false;
				}
			}
			else
				bodyTracked = false;
		}
	}
	
	/// <summary>
	/// Updates the breath breathState based off sample window calculation.
	/// </summary>
	private void UpdateBreath()
	{
		// don't compare apps first reading
		if(previousReading != 0.0f)
		{
			// passes sensitivity threshold
			if(Mathf.Abs(previousReading - currentReading) > Sensitivity())
			{
				if(breathState == BreathState.In && (lengthT / 2) > breathStateChangeBuf)
				{
					if(currentReading < previousReading)
					{
						breathState = BreathState.Out;
						exhaled = true;
					}
				}
				else if(breathState == BreathState.Out && lengthT > breathStateChangeBuf)
				{
					if(currentReading > previousReading)
						breathState = BreathState.In;
				}
			}
		}
	}
	
	/// returns sensitivity as a fractional float
	private float Sensitivity()
	{
		if (!bodyTracked)
			return 0.0f;
		
		float tempF = 0.0f;
		int tempI = 1;
		
		// multiply until in desired range
		while (tempF < 0.2f) 
		{
			tempI++;
			tempF = Mathf.Abs(currentReading) * tempI;
		}
		
		float val = ((1.0f - sensitivity) / 100.0f) / (float)tempI;
		
		return val;
	}
}
