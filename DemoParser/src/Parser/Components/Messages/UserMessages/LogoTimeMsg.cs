using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class LogoTimeMsg : UserMessage {

		public float Time;


		public LogoTimeMsg(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Time = bsr.ReadFloat();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"time: {Time}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("LogTimeLessage");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
