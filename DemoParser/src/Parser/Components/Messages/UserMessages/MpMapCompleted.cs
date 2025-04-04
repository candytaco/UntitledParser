using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class MpMapCompleted : UserMessage {

		public byte Branch; // course probably
		public byte Level;


		public MpMapCompleted(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Branch = bsr.ReadByte();
			Level = bsr.ReadByte();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"branch: {Branch}");
			pw.Append($"level: {Level}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("MPMapCompleted");
			//TODO: this
			parent.Add(thisElement);
		}
	}


	public class MpMapIncomplete : MpMapCompleted {
		public MpMapIncomplete(SourceDemo? demoRef, byte value) : base(demoRef, value) {}
	}
}
