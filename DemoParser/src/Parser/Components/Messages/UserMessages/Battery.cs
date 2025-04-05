using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class Battery : UserMessage {

		// range: 0 - 100
		public ushort BatteryVal;

		public Battery(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			BatteryVal = bsr.ReadUShort();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"Battery: {BatteryVal}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("Battery");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
