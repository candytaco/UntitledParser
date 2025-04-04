using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages {

	public class NetTick : DemoMessage {

		private const float NetTickScaleUp = 100000.0f;

		public uint EngineTick;
		public float? HostFrameTime;
		public float? HostFrameTimeStdDev;


		public NetTick(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			EngineTick = bsr.ReadUInt();
			if (DemoInfo.Game != SourceGame.HL2_OE) {
				HostFrameTime = bsr.ReadUShort() / NetTickScaleUp;
				HostFrameTimeStdDev = bsr.ReadUShort() / NetTickScaleUp;
			}
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"engine tick: {EngineTick}");
			if (DemoInfo.Game != SourceGame.HL2_OE) {
				pw.AppendLine($"\nhost frame time: {HostFrameTime}");
				pw.Append($"host frame time std dev: {HostFrameTimeStdDev}");
			}
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("NetTick", new XAttribute("EngineTick", EngineTick));
			if (DemoInfo.Game != SourceGame.HL2_OE)
				thisElement.Add(new XElement("Host-Frame-Time", HostFrameTime, new XAttribute("Stdev", HostFrameTimeStdDev)));
			parent.Add(thisElement);
		}
	}
}
