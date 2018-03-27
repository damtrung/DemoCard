namespace CardSystem.CMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PrivateKey1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PrivateKey", c => c.String(nullable: false, maxLength: 16));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "PrivateKey");
        }
    }
}
