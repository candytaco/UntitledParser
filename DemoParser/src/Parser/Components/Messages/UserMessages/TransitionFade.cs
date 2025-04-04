using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class TransitionFade : UserMessage {

		public float Seconds;


		public TransitionFade(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Seconds = bsr.ReadFloat();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"seconds: {Seconds}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement(this.GetType().Name);
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
