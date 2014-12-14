using System;
using System.Collections.Generic;

using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules.Music
{
	/// <summary>
	/// Basic implementation of the transition table for music composing.
	/// </summary>
	public class TransitionTableComposer
	{
		/// <summary>
		/// Stores corresondence between pitch and the table's row/column index.
		/// </summary>
		protected Dictionary<string, int> _noteIndexRef;
		/// <summary>
		/// Table to denote probabilities of transitions.
		/// </summary>
		protected float[][] _transitionTable;
		/// <summary>
		/// Helper array, containing names of the available notes.
		/// </summary>
		protected string[] _notes;

		/// <summary>
		/// Initializes a new instance of the <see cref="MentalAlchemy.Molecules.Music.TransitionTableComposer"/> class.
		/// </summary>
		public TransitionTableComposer ()
		{
			FillNoteIndexRef ();
			InitTransitionTable ();
		}

		/// <summary>
		/// Init aux containers.
		/// </summary>
		protected void FillNoteIndexRef ()
		{
			_noteIndexRef = new Dictionary<string, int> ();
			_noteIndexRef.Add (Pitches.C, 0);
			_noteIndexRef.Add (Pitches.Csharp, 1);
			_noteIndexRef.Add (Pitches.D, 2);
			_noteIndexRef.Add (Pitches.Dsharp, 3);
			_noteIndexRef.Add (Pitches.E, 4);
			_noteIndexRef.Add (Pitches.F, 5);
			_noteIndexRef.Add (Pitches.Fsharp, 6);
			_noteIndexRef.Add (Pitches.G, 7);
			_noteIndexRef.Add (Pitches.Gsharp, 8);
			_noteIndexRef.Add (Pitches.A, 9);
			_noteIndexRef.Add (Pitches.Asharp, 10);
			_noteIndexRef.Add (Pitches.B, 11);

			_notes = new string[_noteIndexRef.Count];
			int i = 0;
			foreach (var key in _noteIndexRef.Keys)
			{
				_notes [i] = key;
				++i;
			}
		}

		/// <summary>
		/// Initializes the transition table according to the provided key.
		/// </summary>
		/// <param name="key">Key.</param>
		public void InitTransitionTable (string key = Keys.Cmaj)
		{
			var size = _noteIndexRef.Count;
			_transitionTable = MatrixMath.Zeros (size,size);

			var notes = Keys.GetKeyNotes (key);
			if (notes != null)
			{
				var prob = 1f / (notes.Length);	// default probability of transition.
				for (int i=0; i<notes.Length; ++i)
				{
					var row = _noteIndexRef[notes[i]];
					for (int j=0; j<notes.Length; ++j)
					{
						var col = _noteIndexRef[notes[j]];
						_transitionTable[row][col] = prob;
					}
				}
			}
		}

		/// <summary>
		/// Randomly updates the transition table.
		/// </summary>
		public void ReshaffleTransitionTable ()
		{
			if (_transitionTable == null) InitTransitionTable ();

			var size = _noteIndexRef.Count;
			for (int i=0; i<size; ++i)
			{
				for (int j=0; j<size; ++j)
				{
					if (_transitionTable [i][j] == 0)
						continue;
					_transitionTable [i][j] += (float)ContextRandom.NextDouble ();
					if (_transitionTable [i][j] < 0)
						_transitionTable [i][j] = 0;
				}
				// convert to probabilities.
				_transitionTable [i] = VectorMath.NormalizeSum (_transitionTable[i]);
			}
		}

		/// <summary>
		/// Composes a series of notes or requested length.
		/// </summary>
		/// <param name="startNote">Start note.</param>
		/// <param name="noteCount">Melody length.</param>
		/// <param name="reshufflePeriod">Set to zero to switch off.</param>
		public string[] Compose (string startNote = Pitches.C, int noteCount = 128, int reshufflePeriod = 16)
		{
			var res = new string [noteCount];

			var curNote = startNote;
			for (int i=0; i<noteCount; ++i)
			{
				res [i] = NextNote (curNote);
				curNote = res [i];

				if (i > 0 && reshufflePeriod > 0 && i % reshufflePeriod == 0)
				{
					ReshaffleTransitionTable ();
				}
			}
			return res;
		}

		/// <summary>
		/// Generates the next note from the given one.
		/// </summary>
		/// <returns>The note.</returns>
		/// <param name="prevNote">Previous note.</param>
		public string NextNote (string prevNote)
		{
			var row = _noteIndexRef[prevNote];
			var col = VectorMath.Roulette (_transitionTable[row]);
			return _notes [col];
		}

		/// <summary>
		/// Returns random note according to the transitions table.
		/// </summary>
		/// <returns>The note.</returns>
		public string NextNote ()
		{
			var row = ContextRandom.Next (_notes.Length);
			return NextNote (_notes[row]);
		}
	}
}

