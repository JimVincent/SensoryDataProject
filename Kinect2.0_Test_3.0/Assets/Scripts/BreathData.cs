using UnityEngine;
using System.Collections;
using Windows.Kinect;

public enum breathState {In, Out};

public class BreathData : MonoBehaviour 
{
	/// <summary>
	/// Accessor for KinectManager static Instance
	/// </summary>
	private KinectManager bodyM;

	/// <summary>
	/// The window length that data is acumulated before averaging
	/// </summary>
	[Range(0.1f, 2.0f)]public float sampleTime = 0.3f;

	/// <summary>
	/// How sensitive the system is to movement.
	/// eg. 100 is full sensitivity which may pick up on digital jittering input.
	/// </summary>
	[Range(1, 100)] public int sensitivity = 90;

	/// <summary>
	/// Shortest duration a breath cycle must be to be used for breathLength.
	/// eg. 0.1 seconds may result in rapid in/out state changes which will result in a inaccurate reading.
	/// </summary>
	[Range(0.1f, 2.0f)] public float stateChangeBuf = 1.0f;

	/// <summary>
	/// The current in/out state of the breath
	/// </summary>
	public breathState state = breathState.In;

	/// <summary>
	/// The reading provided from the previous sample window calculation
	/// *Note: Holds a value greater than zero after second sample window has been calculated.
	/// </summary>
	public float previousReading = 0.0f;

	/// <summary>
	/// The most recent sample window calculation
	/// </summary>
	public float currentReading = 0.0f;

	/// <summary>
	/// The duration of the most recent breath cycle.
	/// *Note: This is measured from in breath until the next in breath - 
	/// But only if value is greater than stateChangeBuf.
	/// </summary>
	public float breathLength = 0.0f;

	/// <summary>
	/// Indicates whether a body has been found and is being tracked by the kinect.
	/// *Note: On Kinect start up there is a few seconds before it tracks the users body - 
	/// Also if the user moves out of sight this will result in false.
	/// </summary>
	public bool bodyTracked = false;

	/// <summary>
	/// Accumulates Y pos from both shoulders over the duration of the sample window.
	/// Simple 2 var system instead of using List (performance)
	/// Both variables are reset to zero once sample window calculations are complete.
	/// </summary>
	public float logY = 0.0f; 	// acumulates Y pos during actice reading
	public int logCount = 0; 	// counts log to determine average

	/// <summary>
	/// Timers for tracking durations
	/// </summary>
	private float sampleT = 0.0f;
	public float lengthT = 0.0f;

	/// <summary>
	/// Switch for breath cycle indication
	/// </summary>
	private bool inhaled = false;
	private bool exhaled = false;

	// Use this for initialization
	void Start () 
	{
		// assign static class vars
		bodyM = KinectManager.inst;
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
			currentReading = logY / logCount;
			UpdateBreath();

			// reset logs / timer
			sampleT = logY = logCount = 0;
		}
		else
		{
			// retrieve tracked body
			Body TempBody = bodyM.GetBody();

			if(TempBody != null)
			{
				bodyTracked = true;
				
				// read and log shoulder data
				float leftShoulder = TempBody.Joints[JointType.ShoulderLeft].Position.Y;
				float rightShoulder = TempBody.Joints[JointType.ShoulderRight].Position.Y;
				logY += leftShoulder + rightShoulder;
				logCount++;
				
				// measure breath duration
				if(inhaled)
					lengthT += Time.deltaTime;
				
				// Start of breath cycle
				if(!inhaled && state == breathState.In)
				{
					inhaled = true;
				}
				// End of cycle
				else if(exhaled && state == breathState.In)
				{
					// log the breath duration
					breathLength = lengthT;
					
					// reset timer / inhale switch
					lengthT = 0.0f;
					inhaled = exhaled = false;
				}
			}
			else
				Debug.Log("No bodies are being tracked.");
		}
	}

	/// <summary>
	/// Updates the breath state based off sample window calculation.
	/// </summary>
	private void UpdateBreath()
	{
		// don't compare apps first reading
		if(previousReading != 0.0f)
		{
			// passes sensitivity threshold
			if(Mathf.Abs(previousReading - currentReading) > Sensitivity())
			{
				if(state == breathState.In && (lengthT / 2) > stateChangeBuf)
				{
					if(currentReading < previousReading)
					{
						state = breathState.Out;
						exhaled = true;
					}
				}
				else if(state == breathState.Out && lengthT > stateChangeBuf)
				{
					if(currentReading > previousReading)
						state = breathState.In;
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

		float val = (float)((100 - sensitivity) / 100) / tempI;

		return val;
	}
}
