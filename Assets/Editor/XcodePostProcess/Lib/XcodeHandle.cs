using System;

namespace XcodePostProcessLib
{
	
	public class XcodeHandle
	{
		public string Id { get; private set; }
		public string Isa { get; private set; }
		
		public XcodeHandle(string id, string isa)
		{
			Id = id;
			Isa = isa;
		}
		
		public override string ToString ()
		{
			return string.Format ("[XcodeHandle: Id={0}, Isa={1}]", Id, Isa);
		}
	}
	
	public class XcodeGroup : XcodeHandle
	{
		public XcodeGroup(string id)
			: base(id, "PBXGroup")
		{
		}
	}
	
	public class XcodeFileRef : XcodeHandle
	{
		public XcodeFileRef(string id)
			: base(id, "PBXFileReference")
		{
		}
	}
	
	public class XcodeBuildFile : XcodeHandle
	{
		public XcodeBuildFile(string id)
			: base(id, "PBXBuildFile")
		{
		}
	}
	
}