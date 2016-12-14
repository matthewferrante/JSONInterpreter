namespace PTMS.HttpsAuthProxy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class encryptionkey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "EncryptionKey", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "EncryptionKey");
        }
    }
}
