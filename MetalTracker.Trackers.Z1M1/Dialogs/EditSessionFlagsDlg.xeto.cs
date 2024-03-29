﻿using System;
using System.Linq;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MetalTracker.Trackers.Z1M1.Internal;

namespace MetalTracker.Trackers.Z1M1.Dialogs
{
	internal class EditSessionFlagsDlg : Dialog<bool>
	{
		private CheckBox checkBoxSecondQuest;
		private CheckBox checkBoxMirroredOW;
		private CheckBox checkBoxMirroredDs;
		private CheckBox checkBoxMirroredL1;
		private CheckBox checkBoxMirroredL2;
		private CheckBox checkBoxMirroredL3;
		private CheckBox checkBoxMirroredL4;
		private CheckBox checkBoxMirroredL5;
		private CheckBox checkBoxMirroredL6;
		private CheckBox checkBoxMirroredL7;
		private CheckBox checkBoxMirroredL8;
		private CheckBox checkBoxMirroredL9;
		private CheckBox checkBoxMirroredZebes;
		private CheckBox checkBoxShuffleDungeons;
		private CheckBox checkBoxShuffleOthers;
		private CheckBox checkBoxShuffleMinorDungeonRooms;
		private CheckBox checkBoxShuffleAllDungeonRooms;
		private CheckBox checkBoxShuffleMinorZebesRooms;
		private CheckBox checkBoxShuffleAllZebesRooms;

		public SessionFlags Flags { get; set; }

		private bool _loading;

		public EditSessionFlagsDlg(bool newSession)
		{
			XamlReader.Load(this);

			checkBoxSecondQuest = this.FindChild<CheckBox>("checkBoxSecondQuest");
			checkBoxMirroredOW = this.FindChild<CheckBox>("checkBoxMirroredOW");
			checkBoxMirroredDs = this.FindChild<CheckBox>("checkBoxMirroredDs");
			checkBoxMirroredL1 = this.FindChild<CheckBox>("checkBoxMirroredL1");
			checkBoxMirroredL2 = this.FindChild<CheckBox>("checkBoxMirroredL2");
			checkBoxMirroredL3 = this.FindChild<CheckBox>("checkBoxMirroredL3");
			checkBoxMirroredL4 = this.FindChild<CheckBox>("checkBoxMirroredL4");
			checkBoxMirroredL5 = this.FindChild<CheckBox>("checkBoxMirroredL5");
			checkBoxMirroredL6 = this.FindChild<CheckBox>("checkBoxMirroredL6");
			checkBoxMirroredL7 = this.FindChild<CheckBox>("checkBoxMirroredL7");
			checkBoxMirroredL8 = this.FindChild<CheckBox>("checkBoxMirroredL8");
			checkBoxMirroredL9 = this.FindChild<CheckBox>("checkBoxMirroredL9");
			checkBoxMirroredZebes = this.FindChild<CheckBox>("checkBoxMirroredZebes");
			checkBoxShuffleDungeons = this.FindChild<CheckBox>("checkBoxShuffleDungeons");
			checkBoxShuffleOthers = this.FindChild<CheckBox>("checkBoxShuffleOthers");
			checkBoxShuffleMinorDungeonRooms = this.FindChild<CheckBox>("checkBoxShuffleMinorDungeonRooms");
			checkBoxShuffleAllDungeonRooms = this.FindChild<CheckBox>("checkBoxShuffleAllDungeonRooms");
			checkBoxShuffleMinorZebesRooms = this.FindChild<CheckBox>("checkBoxShuffleMinorZebesRooms");
			checkBoxShuffleAllZebesRooms = this.FindChild<CheckBox>("checkBoxShuffleAllZebesRooms");

			if (newSession)
			{
				this.Title = "Define New Session Flags";
			}
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			_loading = true;

			checkBoxSecondQuest.Checked = Flags.ZeldaQ2;
			checkBoxMirroredOW.Checked = Flags.OverworldMirrored;

			checkBoxMirroredDs.Checked = null;
			if (Flags.DungeonsMirrored.All(d => d == true))
			{
				checkBoxMirroredDs.Checked = true;
			}
			else if (Flags.DungeonsMirrored.All(d => d == false))
			{
				checkBoxMirroredDs.Checked = false;
			}

			checkBoxMirroredL1.Checked = Flags.DungeonsMirrored[0];
			checkBoxMirroredL2.Checked = Flags.DungeonsMirrored[1];
			checkBoxMirroredL3.Checked = Flags.DungeonsMirrored[2];
			checkBoxMirroredL4.Checked = Flags.DungeonsMirrored[3];
			checkBoxMirroredL5.Checked = Flags.DungeonsMirrored[4];
			checkBoxMirroredL6.Checked = Flags.DungeonsMirrored[5];
			checkBoxMirroredL7.Checked = Flags.DungeonsMirrored[6];
			checkBoxMirroredL8.Checked = Flags.DungeonsMirrored[7];
			checkBoxMirroredL9.Checked = Flags.DungeonsMirrored[8];

			checkBoxMirroredZebes.Checked = Flags.ZebesMirrored;
			checkBoxShuffleDungeons.Checked = Flags.DungeonEntrancesShuffled;
			checkBoxShuffleOthers.Checked = Flags.OtherEntrancesShuffled;
			checkBoxShuffleMinorDungeonRooms.Checked = Flags.DungeonRoomShuffleMode > 0;
			checkBoxShuffleAllDungeonRooms.Checked = Flags.DungeonRoomShuffleMode == 2;
			checkBoxShuffleMinorZebesRooms.Checked = Flags.ZebesRoomShuffleMode > 0;
			checkBoxShuffleAllZebesRooms.Checked = Flags.ZebesRoomShuffleMode == 2;

			_loading = false;
 		}

		protected void HandleAllDungeonsMirroredChanged(object sender, EventArgs e)
		{
			if (_loading) return;

			_loading = true;

			if (checkBoxMirroredDs.Checked == true)
			{
				checkBoxMirroredL1.Checked = true;
				checkBoxMirroredL2.Checked = true;
				checkBoxMirroredL3.Checked = true;
				checkBoxMirroredL4.Checked = true;
				checkBoxMirroredL5.Checked = true;
				checkBoxMirroredL6.Checked = true;
				checkBoxMirroredL7.Checked = true;
				checkBoxMirroredL8.Checked = true;
				checkBoxMirroredL9.Checked = true;
			}
			else if (checkBoxMirroredDs.Checked == false)
			{
				checkBoxMirroredL1.Checked = false;
				checkBoxMirroredL2.Checked = false;
				checkBoxMirroredL3.Checked = false;
				checkBoxMirroredL4.Checked = false;
				checkBoxMirroredL5.Checked = false;
				checkBoxMirroredL6.Checked = false;
				checkBoxMirroredL7.Checked = false;
				checkBoxMirroredL8.Checked = false;
				checkBoxMirroredL9.Checked = false;
			}

			_loading = false;
		}

		protected void HandleAllDungeonRoomsCheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxShuffleAllDungeonRooms.Checked == true)
			{
				checkBoxShuffleMinorDungeonRooms.Enabled = false;
				checkBoxShuffleMinorDungeonRooms.Checked = true;
			}
			else
			{
				checkBoxShuffleMinorDungeonRooms.Enabled = true;
				checkBoxShuffleMinorDungeonRooms.Checked = Flags.DungeonRoomShuffleMode > 0;
			}
		}

		protected void HandleAllZebesRoomsCheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxShuffleAllZebesRooms.Checked == true)
			{
				checkBoxShuffleMinorZebesRooms.Enabled = false;
				checkBoxShuffleMinorZebesRooms.Checked = true;
			}
			else
			{
				checkBoxShuffleMinorZebesRooms.Enabled = true;
				checkBoxShuffleMinorZebesRooms.Checked = Flags.ZebesRoomShuffleMode > 0;
			}
		}

		protected void HandleAcceptClick(object sender, EventArgs e)
		{
			Flags.ZeldaQ2 = checkBoxSecondQuest.Checked.Value;
			Flags.OverworldMirrored = checkBoxMirroredOW.Checked.Value;
			Flags.DungeonsMirrored[0] = checkBoxMirroredL1.Checked.Value;
			Flags.DungeonsMirrored[1] = checkBoxMirroredL2.Checked.Value;
			Flags.DungeonsMirrored[2] = checkBoxMirroredL3.Checked.Value;
			Flags.DungeonsMirrored[3] = checkBoxMirroredL4.Checked.Value;
			Flags.DungeonsMirrored[4] = checkBoxMirroredL5.Checked.Value;
			Flags.DungeonsMirrored[5] = checkBoxMirroredL6.Checked.Value;
			Flags.DungeonsMirrored[6] = checkBoxMirroredL7.Checked.Value;
			Flags.DungeonsMirrored[7] = checkBoxMirroredL8.Checked.Value;
			Flags.DungeonsMirrored[8] = checkBoxMirroredL9.Checked.Value;
			Flags.ZebesMirrored = checkBoxMirroredZebes.Checked.Value;
			Flags.DungeonEntrancesShuffled = checkBoxShuffleDungeons.Checked.Value;
			Flags.OtherEntrancesShuffled = checkBoxShuffleOthers.Checked.Value;

			if (checkBoxShuffleAllDungeonRooms.Checked == true)
				this.Flags.DungeonRoomShuffleMode = 2;
			else if (checkBoxShuffleMinorDungeonRooms.Checked == true)
				this.Flags.DungeonRoomShuffleMode = 1;
			else
				this.Flags.DungeonRoomShuffleMode = 0;

			if (checkBoxShuffleAllZebesRooms.Checked == true)
				this.Flags.ZebesRoomShuffleMode = 2;
			else if (checkBoxShuffleMinorZebesRooms.Checked == true)
				this.Flags.ZebesRoomShuffleMode = 1;
			else
				this.Flags.ZebesRoomShuffleMode = 0;

			this.Result = true;

			this.Close();
		}
	}
}
