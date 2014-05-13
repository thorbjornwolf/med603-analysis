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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Experiment1Analysis
{
	public class ExperimentData
	{
		Demographics demographics;
		public List<Participant> participants = new List<Participant>();

		public ExperimentData (string pathToParticipantFolder, double clipObservationDurationMillis)
		{
			string pathToDemographic = Path.Combine(pathToParticipantFolder, "demographic.csv");

			Console.WriteLine("Experiment.pathToDemographic: " + pathToDemographic);

			using(StreamReader stream = new StreamReader(pathToDemographic))
			{
				demographics = new Demographics(stream);
			}

			foreach(DemographicData d in demographics.demoData)
			{
				//Doesn't work before Participant class is done
				participants.Add(new Participant(pathToParticipantFolder, d, clipObservationDurationMillis));
			}
		}

		//Returns the mean of all thresholds per trial, disregarding 

		/// <summary>
		/// Gets the threshold mean for all trials regardless of the number 
		/// of trials connected to each participant.
		/// </summary>
		/// <returns>The threshold mean per trial.</returns>
		public float GetMeanOfThresholdPerTrial() 
		{
			return Statistics.Mean(GetThresholdPerTrial().ToArray());
		}

		/// <summary>
		/// Gets the estimated threshold value from each trials.
		/// </summary>
		/// <returns>The threshold per trial.</returns>
		public List<float> GetThresholdPerTrial()
		{
			List<float> thresholdForEachTrial = new List<float>();
			
			foreach(Participant p in participants)
			{
				foreach(Trial t in p.trials)
				{
					thresholdForEachTrial.Add(t.Threshold);
				}
			}

			return thresholdForEachTrial;
		}

		//Returns the mean of all participant thresholds

		/// <summary>
		/// Gets the mean of all participants' estimated threshold values.
		/// </summary>
		/// <returns>The threshold mean per participant.</returns>
		public float GetMeanOfThresholdPerParticipant()
		{
			return Statistics.Mean(GetThresholdPerParticipant().ToArray());
		}

		/// <summary>
		/// Gets the estimated threshold per participant, regardless of 
		/// how many or how few trials the participant did.
		/// </summary>
		/// <returns>The estimated threshold for each participant.</returns>
		public List<float> GetThresholdPerParticipant()
		{
			List<float> participantThreshold = new List<float>();
			
			foreach(Participant p in participants)
			{
				participantThreshold.Add(p.GetMean());
			}

			return participantThreshold;
		}

		public void DiscardTrialsWithTooFewReverses(int lowestNumberOfReverses)
		{
			foreach(Participant p in participants)
			{
				p.DiscardTrialsWithTooFewReverses(lowestNumberOfReverses);
			}
		}


		public void DiscardTrialsWithTooLargeGazeDistanceMean(float maximumGazeDistanceMean)
		{
			foreach(Participant p in participants)
			{
				p.DiscardTrialsWithTooLargeGazeDistanceMean(maximumGazeDistanceMean);
			}
		}

		// TODO Test or evaluate somehow whether this method does what it should
		public void GetCountsForResponseAround(float stimulusLevel, out int positiveAbove, out int negativeAbove, out int positiveBelow, out int negativeBelow)
		{
			positiveAbove = 0;
			negativeAbove = 0;
			positiveBelow = 0;
			negativeBelow = 0;

			foreach (Participant p in participants) 
			{
				foreach (Trial t in p.trials) 
				{
					foreach(Observation o in t.observations)
					{
						if(o.stimulus >= stimulusLevel)
						{
							// Above
							// Positive
							if(o.response == 1)
							{
								positiveAbove++;
							}
							else
							{
								negativeAbove++;
							}
						}
						else
						{
							// Below
							// Positive
							if(o.response == 1)
							{
								positiveBelow++;
							}
							else
							{
								negativeBelow++;
							}	
						}
					}
				}
			}				
		}

		/// <summary>
		/// Gives a string containng number of participants, number of trials, mean and standard deviation for per-participant thresholds.
		/// Standard deviation is probably not relevant, though.
		/// </summary>
		/// <returns>A quick overview of the stats of this experiment.</returns>
		public string QuickStats()		           
		{
			StringBuilder output = new StringBuilder(100);
			
			// Number of participants
			output.AppendLine(participants.Count + " participants.");
			// Total number of trials
			int trialsCount = 0;
			foreach(Participant p in participants)
			{
				trialsCount += p.trials.Count;
			}
			output.AppendLine(trialsCount + " trials total.");
			
			
			// Average gaze deviation
			List<float> thresholds = new List<float>(participants.Count);
			foreach(Participant p in participants)
			{
				thresholds.Add (p.Threshold());
			}
			
			// Mean threshold
			float mean = Statistics.Mean(thresholds.ToArray());
			output.AppendLine("Mean for thresholds: " + mean);
			
			// Standard deviation threshold
			float sd = Statistics.StandardDeviation(thresholds.ToArray());
			output.AppendLine("Standard deviation for thresholds: " + sd);
			
			
			return output.ToString();
		}

		/// <summary>
		/// Discards participants who 
		/// are deemed uselss
		/// whose standard deviation is above a threshold
		/// Discards trials where
		/// user has looked too much too far away from the marker
		/// number of reverses is below a minimum
		/// Values are hardcoded in the function top
		/// </summary>
		/// <returns>A string describing what actions were taken, and why.</returns>
		public string DiscardBadTrials()
		{
			StringBuilder output = new StringBuilder(100);
			
			float maximumParticipantThresholdSD = 20;
			float maximumGazeDeviationDistance = 300;
			int maximumGazeDeviationExcessCount = 20;
			int minimumNumberOfReverses = 2;
			
			/*
			- Discard users where
				- User is deemed to have had too bad vision (look in notes)
				- Standard deviation among user's trial's estimated thresholds is > some threshold
			*/
			int[] badParticipants = { 1 }; // Participant 1 had bad eyesight
			List<Participant> participantsToDrop = new List<Participant>(1);
			
			foreach(Participant p in participants)
			{
				foreach(int i in badParticipants)
				{
					if(p.demographics.ID == i)
					{
						participantsToDrop.Add(p);
						output.AppendLine("Dropping " + p + " for not fulfilling formal requirements");
					}
				}
			}
			
			foreach(Participant p in participantsToDrop)
			{
				participants.Remove(p);
			}
			participantsToDrop.Clear();
			
			foreach(Participant p in participants)
			{
				if(p.GetStandardDeviation() > maximumParticipantThresholdSD)
				{
					participantsToDrop.Add(p);
					output.AppendLine("Dropping " + p 
					                  + " for being too inconsistent with threshold standard deviation at " 
					                  + p.GetStandardDeviation()
					                  + " for " + p.trials.Count + " trials.");
				}	
			}
			
			foreach(Participant p in participantsToDrop)
			{
				participants.Remove (p);
			}
			participantsToDrop.Clear();
			
			
			// Discard trials by different parameters:
			/*- Discard trials where
				- User is deemed to have looked too much away from marker:
					- max distance > 200 for 10 consecutive readings
						- Number of reverses is < 2
			*/
			List<Trial> trialsToDrop = new List<Trial>(40);
			
			foreach(Participant p in participants)
			{
				foreach(Trial t in p.trials)
				{
					if(t.NumberOfReverses < minimumNumberOfReverses)
					{
						trialsToDrop.Add(t);
						output.AppendLine("Dropping trial " + p.ID + "." + t.ID
						                  + " for having only " + t.NumberOfReverses + " reverses.");
					}
				}
				
				foreach(Trial t in trialsToDrop)
				{
					p.trials.Remove(t);
				}
				trialsToDrop.Clear();
				
				foreach(Trial t in p.trials)
				{
					int max = t.GetMaxNumberOfConsecutiveReadingsWithGazeDistanceAbove(maximumGazeDeviationDistance);
					if(max > maximumGazeDeviationExcessCount)
					{
						trialsToDrop.Add(t);
						output.AppendLine("Dropping trial " + p.ID + "." + t.ID
						                  + " for deviating more than "
						                  + maximumGazeDeviationDistance + " pixels for "
						                  + max + " readings.");
					}
				}
				
				foreach(Trial t in trialsToDrop)
				{
					p.trials.Remove(t);
				}
				trialsToDrop.Clear();
			}
			
			// Clear participants without trials
			foreach(Participant p in participants)
			{
				if(p.trials.Count == 0)
				{
					participantsToDrop.Add(p);
				}
			}
			
			output.AppendLine("Dropping " + participantsToDrop.Count +  " participants for being out of trials:");
			
			foreach(Participant p in participantsToDrop)
			{
				output.AppendLine("\t" + p.ID);
				participants.Remove (p);
			}
			participantsToDrop.Clear();
			
			return output.ToString();
		}
	}
}

