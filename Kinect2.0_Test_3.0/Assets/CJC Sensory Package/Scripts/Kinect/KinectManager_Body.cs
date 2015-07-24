using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Microsoft.Kinect.Face;

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
	private int bodyCount = 6;
	 
	private FaceFrameSource[] faceFrameSources = null;
	private FaceFrameReader[] faceFrameReaders = null;
	private FaceFrameResult[] faceFrameResults = null;

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

//			faceFrameSources = new FaceFrameSource[bodyCount];
//			faceFrameReaders = new FaceFrameReader[bodyCount];
//
//			for(int i = 0; i < bodyCount; ++i)
//			{
//				faceFrameSources[i] = FaceFrameSource.Create(sensor, 0, FaceFrameFeatures.LookingAway);
//				faceFrameReaders[i] = faceFrameSources[i].OpenReader();
//			}
//			
//			faceFrameResults = new FaceFrameResult[bodyCount];
	
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
					bodies = new Body[bodyCount];
					isReady = true;
				}

				// update body data
				frame.GetAndRefreshBodyData(bodies);

				frame.Dispose();
				frame = null;
			}
		}

//		for(int i = 0; i < bodyCount; ++i)
//		{
//			if(bodies != null && bodies[i]!=null)
//			{
//				if(bodies[i].IsTracked)
//				{
//					if(faceFrameReaders[i] != null)
//					{
//						FaceFrame faceFrame = faceFrameReaders[i].AcquireLatestFrame();
//						
//						if(faceFrame != null)
//						{
//
//							DetectionResult result;
//							//if(faceFrame ==null) print ("faceframe");
//							//if(faceFrame.FaceFrameResult ==null) print ("FaceFrameResult");
//							//if(faceFrame.FaceFrameResult.FaceProperties ==null) print ("FaceProperties");
//							if(faceFrame.FaceFrameResult!=null)
//							{
//							//	result = faceFrame.FaceFrameResult.FaceProperties[FaceProperty.LookingAway];
//							//	print(result.ToString());
//							}
//							//faceFrame.Dispose();
//							//faceFrame = null;
//							
//						}
//					}
//				}
//			}


	//	}
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
