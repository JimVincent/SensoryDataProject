using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SensoryGUI : MonoBehaviour 
{

	// Put into it's own class for neatness in the inspector
	[System.Serializable] public class GUIVars
	{
		// UI vars
		public Text bStateText;
		public Text bCycleCountText;
		public Text bLengthText;
		public Text bAvLengthText;
		public Text isTrackingText;
		public Image statePanel;
	}

	public string toggleKey = "k"; // Allows user to customise their toggle key
	public Canvas canvas; // For toggling the canvas in one line of code
	public GUIVars ui; // Creates instance of the above class
 	public bool panelOpen = false; // Bool the user can toggle in the inspector

	// static variable accessor
	BreathData breath;

	// Use this for initialization
	void Start () 
	{
		// assign local variable to static instance reference
		breath = BreathData.inst;

		// if the bool is false when the scene starts, disable the canvas
		if(!panelOpen)
		{
			canvas.enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(panelOpen)
		{
			// check that the data is ready
			if(breath.GetIsTracking())
			{
				// change colour depending on breath state
				if(breath.GetBreathState() == BreathState.In)
					ui.statePanel.color = Color.blue;
				else
					ui.statePanel.color = Color.red;

				// retrieve each of the available data values
				ui.bStateText.text = "Breathing " + breath.GetBreathState().ToString();
				ui.bCycleCountText.text = "Breath Cycles Count: " + breath.GetTotalBreathCycles().ToString();
				ui.bLengthText.text = "Recent Breath Length: " + breath.GetBreathLength().ToString();
				ui.bAvLengthText.text = "Average Breath Length: " + breath.GetAverageBreathLength().ToString();

				ui.isTrackingText.text = "Is Tracking: Yes";
			}
			else
				ui.isTrackingText.text = "Is Tracking: No";
		}

		// check to see if a toggleKey has been entered
		if(toggleKey != "")
		{
			// Check if the toggle key is pressed
			if(Input.GetKeyDown(toggleKey))
			{
				// Toggle the panel
				if(!panelOpen)
				{
					panelOpen = true;
					canvas.enabled = true;
				}
				else
				{
					panelOpen = false;
					canvas.enabled = false;
				}
			}
		}

	}

	// UI Refresh Button was pressed
	public void ButtonPress()
	{
		breath.RefreshBreath ();
	}
}
