using System.Numerics;
using System.Xml.Linq;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Messages {

	public class SvcFixAngle : DemoMessage {

		public bool Relative;
		public Vector3 Angle;

		public SvcFixAngle(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Relative = bsr.ReadBool();
			Angle = new Vector3 {
				X = bsr.ReadBitAngle(16),
				Y = bsr.ReadBitAngle(16),
				Z = bsr.ReadBitAngle(16),
			};
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"relative: {Relative}");
			pw.Append($"Angle: <{Angle.X:F4}°, {Angle.Y:F4}°, {Angle.Z:F4}°>");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = XMLHelper.MakeVect3Element("SvcFixAngle", Angle);
			thisElement.Add(new XAttribute("Relative", Relative));
			parent.Add(thisElement);
		}
	}
}
