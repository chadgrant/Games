using System;
using GameStudio.Repository;

namespace GameStudio.Games
{
	public class ReferenceItem: IEquatable<ReferenceItem>, IAudit, IAuditor
	{
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public DateTime? Updated { get; set; }
		public string CreatedBy { get; set; }
		public string UpdatedBy { get; set; }

		public bool Equals(ReferenceItem other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ReferenceItem) obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}