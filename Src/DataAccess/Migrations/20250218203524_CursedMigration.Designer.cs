﻿// <auto-generated />
using System;
using DataAccess.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(OmmelSamvirkeDbContext))]
    [Migration("20250218203524_CursedMigration")]
    partial class CursedMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CampaignCleanedRecipient", b =>
                {
                    b.Property<int>("CampaignId")
                        .HasColumnType("int");

                    b.Property<int>("RecipientId")
                        .HasColumnType("int");

                    b.HasKey("CampaignId", "RecipientId");

                    b.HasIndex("RecipientId");

                    b.ToTable("CampaignCleanedRecipients", (string)null);
                });

            modelBuilder.Entity("CampaignUncleanedRecipient", b =>
                {
                    b.Property<int>("CampaignId")
                        .HasColumnType("int");

                    b.Property<int>("RecipientId")
                        .HasColumnType("int");

                    b.HasKey("CampaignId", "RecipientId");

                    b.HasIndex("RecipientId");

                    b.ToTable("CampaignUncleanedRecipients", (string)null);
                });

            modelBuilder.Entity("ContactListRecipient", b =>
                {
                    b.Property<int>("ContactListId")
                        .HasColumnType("int");

                    b.Property<int>("RecipientId")
                        .HasColumnType("int");

                    b.HasKey("ContactListId", "RecipientId");

                    b.HasIndex("RecipientId");

                    b.ToTable("ContactListRecipients", (string)null);
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.Attachment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContentPath")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<int>("EmailId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("EmailId");

                    b.ToTable("Attachments", (string)null);
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.ContactList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<Guid>("UnsubscribeToken")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("ContactLists", (string)null);
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.ContactListUnsubscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ContactListId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("UndoToken")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("ContactListUnsubscriptions", (string)null);
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.DailyContactListAnalytics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContactListName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsNewsletter")
                        .HasColumnType("bit");

                    b.Property<int>("TotalContacts")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DailyContactListAnalytics", (string)null);
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.DailyEmailAnalytics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<int>("SentEmails")
                        .HasColumnType("int");

                    b.Property<int>("TotalRecipients")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DailyEmailAnalytics", (string)null);
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.Email", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("HtmlBody")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PlainTextBody")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderEmailAddress")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.HasKey("Id");

                    b.HasIndex("DateCreated");

                    b.ToTable("Emails", (string)null);
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.Recipient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("Token")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.HasKey("Id");

                    b.HasIndex("EmailAddress")
                        .IsUnique();

                    b.HasIndex("Token");

                    b.ToTable("Recipients", (string)null);
                });

            modelBuilder.Entity("DomainModules.Newsletters.Entities.Newsletter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<int>("EmailId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DateCreated");

                    b.HasIndex("DateModified");

                    b.HasIndex("EmailId")
                        .IsUnique();

                    b.ToTable("Newsletters", (string)null);
                });

            modelBuilder.Entity("DomainModules.Newsletters.Entities.NewsletterGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ContactListId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.HasKey("Id");

                    b.HasIndex("ContactListId")
                        .IsUnique();

                    b.HasIndex("Name");

                    b.ToTable("NewsletterGroups", (string)null);
                });

            modelBuilder.Entity("DomainModules.Newsletters.Entities.NewsletterGroupsCleanupCampaign", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CampaignDurationMonths")
                        .HasColumnType("int");

                    b.Property<DateTime>("CampaignStart")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsCampaignStarted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastReminderSent")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CampaignStart");

                    b.ToTable("NewsletterGroupsCleanupCampaigns", (string)null);
                });

            modelBuilder.Entity("DomainModules.Newsletters.Entities.NewsletterSubscriptionConfirmation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ConfirmationExpiry")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ConfirmationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ConfirmationToken")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsConfirmed")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("ConfirmationExpiry");

                    b.ToTable("NewsletterSubscriptionConfirmations", (string)null);
                });

            modelBuilder.Entity("DomainModules.Newsletters.Entities.NewsletterUnsubscribeConfirmation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ConfirmationExpiry")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ConfirmationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ConfirmationToken")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateModified")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsConfirmed")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("ConfirmationExpiry");

                    b.ToTable("NewsletterUnsubscribeConfirmations", (string)null);
                });

            modelBuilder.Entity("EmailRecipient", b =>
                {
                    b.Property<int>("EmailId")
                        .HasColumnType("int");

                    b.Property<int>("RecipientId")
                        .HasColumnType("int");

                    b.HasKey("EmailId", "RecipientId");

                    b.HasIndex("RecipientId");

                    b.ToTable("EmailRecipients", (string)null);
                });

            modelBuilder.Entity("NewsletterNewsletterGroup", b =>
                {
                    b.Property<int>("NewsletterId")
                        .HasColumnType("int");

                    b.Property<int>("NewsletterGroupId")
                        .HasColumnType("int");

                    b.HasKey("NewsletterId", "NewsletterGroupId");

                    b.HasIndex("NewsletterGroupId");

                    b.ToTable("NewsletterNewsletterGroups", (string)null);
                });

            modelBuilder.Entity("CampaignCleanedRecipient", b =>
                {
                    b.HasOne("DomainModules.Newsletters.Entities.NewsletterGroupsCleanupCampaign", null)
                        .WithMany()
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DomainModules.Emails.Entities.Recipient", null)
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CampaignUncleanedRecipient", b =>
                {
                    b.HasOne("DomainModules.Newsletters.Entities.NewsletterGroupsCleanupCampaign", null)
                        .WithMany()
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DomainModules.Emails.Entities.Recipient", null)
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ContactListRecipient", b =>
                {
                    b.HasOne("DomainModules.Emails.Entities.ContactList", null)
                        .WithMany()
                        .HasForeignKey("ContactListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DomainModules.Emails.Entities.Recipient", null)
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.Attachment", b =>
                {
                    b.HasOne("DomainModules.Emails.Entities.Email", "Email")
                        .WithMany("Attachments")
                        .HasForeignKey("EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Email");
                });

            modelBuilder.Entity("DomainModules.Newsletters.Entities.Newsletter", b =>
                {
                    b.HasOne("DomainModules.Emails.Entities.Email", "Email")
                        .WithOne()
                        .HasForeignKey("DomainModules.Newsletters.Entities.Newsletter", "EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Email");
                });

            modelBuilder.Entity("DomainModules.Newsletters.Entities.NewsletterGroup", b =>
                {
                    b.HasOne("DomainModules.Emails.Entities.ContactList", "ContactList")
                        .WithOne()
                        .HasForeignKey("DomainModules.Newsletters.Entities.NewsletterGroup", "ContactListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ContactList");
                });

            modelBuilder.Entity("EmailRecipient", b =>
                {
                    b.HasOne("DomainModules.Emails.Entities.Email", null)
                        .WithMany()
                        .HasForeignKey("EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DomainModules.Emails.Entities.Recipient", null)
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NewsletterNewsletterGroup", b =>
                {
                    b.HasOne("DomainModules.Newsletters.Entities.NewsletterGroup", null)
                        .WithMany()
                        .HasForeignKey("NewsletterGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DomainModules.Newsletters.Entities.Newsletter", null)
                        .WithMany()
                        .HasForeignKey("NewsletterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DomainModules.Emails.Entities.Email", b =>
                {
                    b.Navigation("Attachments");
                });
#pragma warning restore 612, 618
        }
    }
}
