// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;


namespace Experiment1Analysis
{
	/*
	Reads from a string
	points to the folder
	
	 */
	public class Participant
	{
		public List<Trial> trials;
		public DemographicData demographics;

		public int ID
		{
			get
			{
				return demographics.ID;
			}
		}

		// TODO Test if this constructor works properly with real data. Test Trial contents, and ensure the connection between demographics and trial data is correct.
		public Participant (string basePath, DemographicData demographics, double clipObservationDurationMillis)
		{
			this.demographics = demographics;

			string path = GetParticipantSpecificPath(basePath, demographics.ID);

			// Pair observation and gazelog files
			string[] files = Directory.GetFiles(path);

			List<string> observationFiles = new List<string>(4);
			List<string> gazeLogFiles = new List<string>(4);

			foreach(string s in files)
			{
				if(s.Contains("gazelog"))
				{
					gazeLogFiles.Add(s);
				}
				else
				{
					observationFiles.Add(s);
				}
			}

			observationFiles.Sort(); // To proper order!
			gazeLogFiles.Sort();

			if(observationFiles.Count == 0)
			{
				throw new InvalidDataException("Number of observation files is 0 " +
				                               "in path " + path);
			}
			if( gazeLogFiles.Count == 0)
			{
				throw new InvalidDataException("Number of gazelog files is 0 " +
				                               "in path " + path);
			}
			if(observationFiles.Count != gazeLogFiles.Count)
			{
				throw new InvalidDataException("Number of observation files (" + observationFiles.Count + 
				                               ") not matching number of gazelog files (" + gazeLogFiles.Count +
				                               ") in path " + path);
			}

			trials = new List<Trial>(4);

			for(int i = 0; i < observationFiles.Count; i++)
			{
				using(StreamReader sro = new StreamReader(observationFiles[i]))
				{
					using(StreamReader srg = new StreamReader(gazeLogFiles[i]))
					{
						//Console.WriteLine("adding trial for " + this.ToString());
						trials.Add(new Trial(sro, srg, i + 1, clipObservationDurationMillis));
					}
				}
			}
		}

		public void DiscardTrialsWithTooFewReverses(int lowestNumberOfReverses)
		{
			List<Trial> toBeRemoved = new List<Trial>();

			foreach(Trial t in trials)
			{
				if(t.NumberOfReverses < lowestNumberOfReverses)
				{
					toBeRemoved.Add (t);
				}
			}

			foreach(Trial t in toBeRemoved)
			{
				Console.WriteLine(this.ToString() + " removing " + t);
				trials.Remove(t);
			}
		}

		public void DiscardTrialsWithTooLargeGazeDistanceMean(float maximumGazeDistanceMean)
		{
			List<Trial> toBeRemoved = new List<Trial>();
			
			foreach(Trial t in trials)
			{
				if(t.GetMeanGazeDistance() > maximumGazeDistanceMean)
				{
					toBeRemoved.Add (t);
				}
			}
			
			foreach(Trial t in toBeRemoved)
			{
				Console.WriteLine(this.ToString() + " removing " + t);
				trials.Remove(t);
			}
		}

		public static string GetParticipantSpecificPath(string basePath, int ID)
		{
			string folderName = ID.ToString("0000"); // 4 digits in the folder name
			return Path.Combine(basePath, folderName);
		}

		/// <summary>
		/// Returns the mean of the participant's estimated threshold values
		/// </summary>
		/// <returns>The mean.</returns>
		public float GetMean()
		{
			List<float> thresholdList = new List<float>();

			foreach(Trial t in trials)
			{
				thresholdList.Add(t.Threshold);
			}

			return Statistics.Mean(thresholdList.ToArray());
		}

		public float Threshold()
		{
			return GetMean();
		}

		public float GetStandardDeviation()
		{
			List<float> thresholdList = new List<float>();
			
			foreach(Trial t in trials)
			{
				thresholdList.Add(t.Threshold);
			}
			
			return Statistics.StandardDeviation(thresholdList.ToArray());
		}

		public override string ToString ()
		{
			return string.Format ("Participant " + demographics.ID);
		}

		public List<Observation> ConcatenateObservations()
		{
			List<Observation> output = new List<Observation>(4*10);
			foreach(Trial t in trials)
			{
				output.AddRange(t.observations);
			}

			return output;
		}

		public List<float> ConcatenateStimuli()
		{
			List<float> output = new List<float>(4*10);

			foreach(Trial t in trials)
			{
				output.AddRange(t.ConcatenateStimuli());
			}
			
			return output;
		}

		public Trial GetTrial(int ID)
		{
			Trial output = null;
			foreach(Trial t in trials)
			{
				if(t.ID == ID)
				{
					output = t;
					break;
				}
			}
			return output;
		}
	}
}

