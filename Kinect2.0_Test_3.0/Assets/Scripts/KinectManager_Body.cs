using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class KinectManager_Body : MonoBehaviour 
{
	// static vars
	public static KinectManager_Body inst;

	// public vars
	public bool isReady = false;	

	// private vars
	private KinectSensor sensor;
	private BodyFrameReader bodyReader;
	private Body[] bodies = null;

	// returns the user body
	public Body GetBody()
	{
		if(isReady)
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
			bodyReader = sensor.BodyFrameSource.OpenReader();
	
			if (!sensor.IsOpen) 
				sensor.Open ();
		} 
		else
			Debug.Log ("No Kinect Sensor found. Check connections / power");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (bodyReader != null) 
		{
			BodyFrame frame = bodyReader.AcquireLatestFrame ();

			if (frame != null) 
			{
				// create instance of bodies
				if (bodies == null) 
				{
					bodies = new Body[sensor.BodyFrameSource.BodyCount];
					isReady = true;
				}

				// update body data
				frame.GetAndRefreshBodyData(bodies);

				frame.Dispose();
				frame = null;
			}
		}
	}
	// shut the kinect down
	void OnApplicationQuit()
	{
		if (bodyReader != null)
		{
			bodyReader.Dispose();
			bodyReader = null;
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
