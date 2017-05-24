using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Clock : MonoBehaviour 
{
	public bool continuous;

	public Transform hour;
	public Transform minute;
	public Transform second;

	private const float perHourRotation = 360f / 12f;
	private const float perMinuteRotation = 360f / 60f;
	private const float perSecondRotation = 360f / 60f; 

	void Start()
	{
		Debug.Log("Oldest age is : " + DateTime.MinValue);
		Debug.Log("Farthest future is " + DateTime.MaxValue);
		Debug.Log("Current time is " + DateTime.Now);

		DateTime myTime = new DateTime(1990, 7, 19, 0, 6, 6, DateTimeKind.Utc);
		Debug.Log("My custom time is " + myTime);

		TimeSpan span = myTime.TimeOfDay;
		Debug.Log(span.TotalHours);
	}

	void Update()
	{
		if(continuous)
		{
			TimeSpan span = DateTime.Now.TimeOfDay;	//the elapsed time of today

			//Debug.Log(span.TotalSeconds);
			hour.localRotation = Quaternion.Euler(0f, 0f, -(float)span.TotalHours * perHourRotation);
			minute.localRotation = Quaternion.Euler(0f, 0f, -(float)span.TotalMinutes * perMinuteRotation);
			second.localRotation = Quaternion.Euler(0f, 0f, -(float)span.TotalSeconds * perSecondRotation);
		}else
		{
			DateTime time = DateTime.Now;

			hour.localRotation = Quaternion.Euler(0f, 0f, -time.Hour * perHourRotation);
			minute.localRotation = Quaternion.Euler(0f, 0f, -time.Minute * perMinuteRotation);
			second.localRotation = Quaternion.Euler(0f, 0f, -time.Second * perSecondRotation);	
		}
	}
}
