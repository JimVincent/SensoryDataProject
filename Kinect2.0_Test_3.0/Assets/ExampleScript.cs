using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExampleScript : MonoBehaviour 
{
	// static variable accessor
	BreathData breath;

	// UI vars
	public Text bStateText;
	public Text bLengthText;
	public Text bAvLengthText;
	public Text isTrackingText;
	public Image statePanel;

	// Use this for initialization
	void Start () 
	{
		// assign local variable to static instance reference
		breath = BreathData.inst;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// check that the data is ready
		if(breath.GetIsTracking())
		{
			// change colour depending on breath state
			if(breath.GetBreathState() == BreathState.In)
				statePanel.color = Color.blue;
			else
				statePanel.color = Color.red;

			// retrieve each of the available data values
			bStateText.text = "Breathing " + breath.GetBreathState().ToString();
			bLengthText.text = "Recent Breath Length: " + breath.GetBreathLength().ToString();
			bAvLengthText.text = "Average Breath Length: " + breath.GetAverageBreathLength().ToString();

			isTrackingText.text = "Is Tracking: Yes";
		}
		else
			isTrackingText.text = "Is Tracking: No";

	}

	// UI Refresh Button was pressed
	public void ButtonPress()
	{
		breath.RefreshBreath ();
	}
}
