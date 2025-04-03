using System;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Packets {

	/// <summary>
	/// Contains direct user input info such as mouse delta and buttons pressed.
	/// </summary>
	public class UserCmd : DemoPacket {

		public uint Cmd;
		public uint? CommandNumber;
		public uint? TickCount;
		public float? ViewAngleX, ViewAngleY, ViewAngleZ;
		public float? SidewaysMovement, ForwardMovement, VerticalMovement;
		public Buttons? Buttons;
		public byte? Impulse;
		public uint? WeaponSelect, WeaponSubtype;
		public short? MouseDx, MouseDy;

		public UserCmd(SourceDemo? demoRef, PacketFrame frameRef) : base(demoRef, frameRef) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Cmd = bsr.ReadUInt();
			int byteSize = bsr.ReadSInt();
			BitStreamReader uBsr = bsr.ForkAndSkip(byteSize * 8);
			CommandNumber = uBsr.ReadUIntIfExists();
			TickCount = uBsr.ReadUIntIfExists();
			ViewAngleX = uBsr.ReadFloatIfExists();
			ViewAngleY = uBsr.ReadFloatIfExists();
			ViewAngleZ = uBsr.ReadFloatIfExists();
			SidewaysMovement = uBsr.ReadFloatIfExists();
			ForwardMovement = uBsr.ReadFloatIfExists();
			VerticalMovement = uBsr.ReadFloatIfExists();
			Buttons = (Buttons?)uBsr.ReadUIntIfExists();
			Impulse = uBsr.ReadByteIfExists();
			if (uBsr.ReadBool()) {
				WeaponSelect = uBsr.ReadUInt(11);
				WeaponSubtype = uBsr.ReadUIntIfExists(6);
			}
			MouseDx = (short?)uBsr.ReadUShortIfExists();
			MouseDy = (short?)uBsr.ReadUShortIfExists();
			if (uBsr.HasOverflowed)
				DemoRef.LogError($"{GetType().Name}: reader overflowed");
			TimingAdjustment.AdjustFromUserCmd(this);
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"cmd: {Cmd}\n");
			pw.Append($"command number: {CommandNumber}\n");
			pw.Append($"tick count: {TickCount}\n");
			pw.AppendFormat("view angles: {0,11} {1,11} {2,11}\n",
				$"{(ViewAngleX.HasValue ? $"{ViewAngleX.Value:F2}°" : "null ")},",
				$"{(ViewAngleY.HasValue ? $"{ViewAngleY.Value:F2}°" : "null ")},",
				$"{(ViewAngleZ.HasValue ? $"{ViewAngleZ.Value:F2}°" : "null ")}");
			pw.AppendFormat("movement:    {0,11} {1,11} {2,11}\n",
				$"{(SidewaysMovement.HasValue ? $"{SidewaysMovement.Value:F2}" : "null")} ,",
				$"{(ForwardMovement.HasValue  ? $"{ForwardMovement.Value:F2}" : "null")} ,",
				$"{(VerticalMovement.HasValue ? $"{VerticalMovement.Value:F2}" : "null")} ");
			pw.Append($"buttons: {Buttons?.ToString() ?? "null"}\n");
			pw.Append($"impulse: {Impulse?.ToString() ?? "null"}\n");
			pw.AppendFormat("weapon, subtype:  {0,4}, {1,4}\n",
				WeaponSelect?.ToString() ?? "null", WeaponSubtype?.ToString() ?? "null");
			pw.AppendFormat("mouseDx, mouseDy: {0,4}, {1,4}",
				MouseDx?.ToString() ?? "null", MouseDy?.ToString() ?? "null");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("UserCommand",
				new XAttribute("Command_number", CommandNumber.Value),
				new XAttribute("Tick-Count", TickCount.Value));
			if (ViewAngleX.HasValue || ViewAngleY.HasValue || ViewAngleZ.HasValue)
				thisElement.Add(new XElement("View-Angles", String.Format("{0},{1},{2}", ViewAngleX, ViewAngleY, ViewAngleZ)));
			if (SidewaysMovement.HasValue || ForwardMovement.HasValue || VerticalMovement.HasValue)
				thisElement.Add(new XElement("Movement", String.Format("{0},{1},{2}", SidewaysMovement, ForwardMovement, VerticalMovement)));
			if (Buttons.HasValue)
				thisElement.Add(new XElement("Buttons", Buttons.ToString()));
			if (Impulse.HasValue)
				thisElement.Add(new XElement("Impulse", Impulse.ToString()));
			if (WeaponSelect.HasValue)
			{
				XElement weapon = new XElement("WeaponSelect", WeaponSelect.ToString());
				if (WeaponSubtype.HasValue)
					weapon.Add(new XAttribute("Subtype", WeaponSubtype.ToString()));
				thisElement.Add(weapon);
			}
			if (MouseDx.HasValue ||  MouseDy.HasValue)
				thisElement.Add(new XElement("Mouse", new XAttribute("dX", MouseDx), new XAttribute("dY", MouseDy)));
			parent.Add(thisElement);
		}
	}


	// https://github.com/alliedmodders/hl2sdk/blob/portal2/game/shared/in_buttons.h
	// se2007/game/shared/in_buttons.h
	[Flags]
	public enum Buttons {
		None            = 0,
		Attack          = 1,
		Jump            = 1 << 1,
		Duck            = 1 << 2,
		Forward         = 1 << 3,
		Back            = 1 << 4,
		Use             = 1 << 5,
		Cancel          = 1 << 6,
		Left            = 1 << 7,
		Right           = 1 << 8,
		MoveLeft        = 1 << 9,
		MoveRight       = 1 << 10,
		Attack2         = 1 << 11,
		Run             = 1 << 12,
		Reload          = 1 << 13,
		Alt1            = 1 << 14,
		Alt2            = 1 << 15,
		Score           = 1 << 16,
		Speed           = 1 << 17,
		Walk            = 1 << 18,
		Zoom            = 1 << 19,
		Weapon1         = 1 << 20,
		Weapon2         = 1 << 21,
		BullRush        = 1 << 22,
		Grenade1        = 1 << 23,
		Grenade2        = 1 << 24,
		LookSpin        = 1 << 25,
		CurrentAbility  = 1 << 26,
		PreviousAbility = 1 << 27,
		Ability1        = 1 << 28,
		Ability2        = 1 << 29,
		Ability3        = 1 << 30,
		Ability4        = 1 << 31
	}
}
