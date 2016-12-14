namespace PTMS.HttpsAuthProxy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPracticeId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PracticeId", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "PracticeId");
        }
    }
}
