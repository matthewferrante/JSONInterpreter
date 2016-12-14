namespace PTMS.HttpsAuthProxy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProxyPasscode2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Passcode", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Passcode");
        }
    }
}
