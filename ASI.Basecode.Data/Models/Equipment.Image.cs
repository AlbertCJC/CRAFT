namespace ASI.Basecode.Data.Models;

/// <summary>
/// Extra equipment fields added by CRAFT on top of the scaffolded equipment table.
/// Kept in a partial so re-scaffolding the database-first model never clobbers it.
/// </summary>
public partial class Equipment
{
    /// <summary>Web-relative path to the uploaded equipment photo (e.g. /uploads/equipment/x.png).</summary>
    public string ImagePath { get; set; }
}
