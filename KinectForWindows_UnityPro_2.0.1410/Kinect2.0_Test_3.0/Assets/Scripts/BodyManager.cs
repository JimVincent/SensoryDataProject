using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class BodyManager : MonoBehaviour 
{
	// static vars
	public static BodyManager inst;

	// public vars
	public bool Connected = false;	

	// private vars
	private KinectSensor sensor;
	private BodyFrameReader reader;
	private Body[] bodies = null;

	// returns the user body
	public Body GetBody()
	{
		if(Connected)
		{
			for(int i = 0; i < bodies.Length; ++i)
			{
				if(bodies[i].IsTracked)
					return bodies[i];
			}
		}

		return null;
	}

	void Awake()
	{	// create static instance

		if(inst == null)
			inst = this;
	}

	// Use this for initialization
	void Start () 
	{
		// set up kinect connection
		sensor = KinectSensor.GetDefault();

		if (sensor != null) 
		{
			reader = sensor.BodyFrameSource.OpenReader ();
			
			if (!sensor.IsOpen) 
				sensor.Open ();
		} 
		else
			Debug.Log ("No Kinect Sensor found. Check connections / power");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (reader != null) 
		{
			BodyFrame frame = reader.AcquireLatestFrame ();

			if (frame != null) 
			{
				if (bodies == null) 
				{
					bodies = new Body[sensor.BodyFrameSource.BodyCount];
					Connected = true;
				}

				// occupy bodies
				frame.GetAndRefreshBodyData(bodies);
				
				frame.Dispose();
				frame = null;
			}
		}
	}

	void OnApplicationQuit()
	{
		if (reader != null)
		{
			reader.Dispose();
			reader = null;
		}
		
		if (sensor != null)
		{
			if (sensor.IsOpen)
			{
				sensor.Close();
			}
			
			sensor = null;
		}
	}
}
