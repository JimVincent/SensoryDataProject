using UnityEngine;
using System.Collections;
using Windows.Kinect;

public enum breathState {In, Out};

public class BreathData : MonoBehaviour 
{
	// static class vars
	private BodyManager bodyM;

	[Range(0.1f, 2.0f)]public float sampleTime = 0.3f;
	[Range(1, 100)] public int sensitivity = 90;

	public breathState state;
	public float previousReading = 0.0f;
	public float currentReading = 0.0f;

	// simple 2 var system instead of ysing List (performance)
	public float logY = 0.0f; 	// acumulates Y pos during actice reading
	public int logCount = 0; 	// counts log to determine average

	// timers
	private float sampleT = 0.0f;

	// Use this for initialization
	void Start () 
	{
		// assign static class vars
		bodyM = BodyManager.inst;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// inc timers
		sampleT += Time.deltaTime;

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
				// read and log shoulder data
				float leftShoulder = TempBody.Joints[JointType.ShoulderLeft].Position.Y;
				float rightShoulder = TempBody.Joints[JointType.ShoulderRight].Position.Y;
				float spineShoulder = TempBody.Joints[JointType.SpineShoulder].Position.Y;

				logY += leftShoulder + rightShoulder + spineShoulder;
				logCount++;
			}
			else
				Debug.Log("No body being tracked");
		}
	}
	
	private void UpdateBreath()
	{
		// don't compare apps first reading
		if(previousReading != 0.0f)
		{
			// passes sensitivity threshold
			if(Mathf.Abs(previousReading - currentReading) > Sensitivity())
			{
				if(currentReading > previousReading)
					state = breathState.In;
				else
					state = breathState.Out;
			}
		}
	}

	/// returns sensitivity as a fractional float
	private float Sensitivity()
	{
		return (float)(100 - sensitivity) / 100000;
	}
}
