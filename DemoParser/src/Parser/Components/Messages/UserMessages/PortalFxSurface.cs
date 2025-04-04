using System.Numerics;
using System.Xml.Linq;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class PortalFxSurface : UserMessage {

		public ushort PortalEnt;
		public ushort OwnerEnt;
		public byte Team;
		public byte PortalNum;
		public PortalFizzleType Effect;
		public Vector3 Origin;
		public Vector3 Angles;


		public PortalFxSurface(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			PortalEnt = bsr.ReadUShort();
			OwnerEnt = bsr.ReadUShort();
			Team = bsr.ReadByte();
			PortalNum = bsr.ReadByte();
			Effect = (PortalFizzleType)bsr.ReadByte();
			bsr.ReadVectorCoord(out Origin);
			bsr.ReadVectorCoord(out Angles);
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"portal entity: {PortalEnt}");
			pw.AppendLine($"owner entity: {OwnerEnt}");
			pw.AppendLine($"team: {Team}");
			pw.AppendLine($"portal num: {PortalNum}");
			pw.AppendLine($"effect: {Effect}");
			pw.AppendLine($"origin: {Origin}");
			pw.Append($"angles: {Angles}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("PortalFXSurface");
			thisElement.Add(new XElement("Portal-Entity",PortalEnt));
			thisElement.Add(new XElement("Owner-Entity",OwnerEnt));
			thisElement.Add(new XElement("Team",Team));
			thisElement.Add(new XElement("PortalNum",PortalNum));
			thisElement.Add(new XElement("Effect",Effect));
			thisElement.Add(XMLHelper.MakeVect3Element("Origin", Origin));
			thisElement.Add(XMLHelper.MakeVect3Element("Angles", Angles));
			parent.Add(thisElement);
		}
	}


	public enum PortalFizzleType : byte {
		PortalFizzleSuccess = 0, // Placed fine (no fizzle)
		PortalFizzleCantFit,
		PortalFizzleOverlappedLinked,
		PortalFizzleBadVolume,
		PortalFizzleBadSurface,
		PortalFizzleKilled,
		PortalFizzleCleanser,
		PortalFizzleClose,
		PortalFizzleNearBlue,
		PortalFizzleNearRed,
		PortalFizzleNone,
	}
}
