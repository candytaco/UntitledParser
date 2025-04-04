using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class KillCam : UserMessage {

		public SpectatorMode SpecMode;
		public byte Target1;
		public byte Target2;
		public byte Unknown;


		public KillCam(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			SpecMode = (SpectatorMode)bsr.ReadByte();
			Target1 = bsr.ReadByte();
			Target2 = bsr.ReadByte();
			Unknown = bsr.ReadByte();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"spectator mode: {SpecMode}");
			pw.AppendLine($"target 1: {Target1}");
			pw.AppendLine($"target 2: {Target2}");
			pw.Append($"unknown: {Unknown}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("KillCam");
			thisElement.Add(new XElement("SpecMode", SpecMode));
			thisElement.Add(new XElement("Target-1", Target1));
			thisElement.Add(new XElement("Target-2", Target2));
			thisElement.Add(new XElement("Unknown-Item", Unknown));
			parent.Add(thisElement);
		}
	}


	// these are compared to new mode, but I don't think that's what it corresponds to
	public enum SpectatorMode : byte {
		None,      // not in spectator mode
		DeathCam,  // special mode for death cam animation
		FreezeCam, // zooms to a target, and freeze-frames on them
		Fixed,     // view from a fixed camera position
		InEye,     // follow a player in first person view
		Chase,     // follow a player in third person view
		Roaming    // free roaming
	}
}
