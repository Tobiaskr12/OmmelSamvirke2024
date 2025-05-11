using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockedReservationTimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedReservationTimeSlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UnsubscribeToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactListUnsubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UndoToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContactListId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactListUnsubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyContactListAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactListName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalContacts = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsNewsletter = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyContactListAnalytics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyEmailAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentEmails = table.Column<int>(type: "int", nullable: false),
                    TotalRecipients = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyEmailAnalytics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderEmailAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlainTextBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsNewsletter = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventCoordinators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCoordinators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterGroupsCleanupCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastReminderSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CampaignDurationMonths = table.Column<int>(type: "int", nullable: false),
                    IsCampaignStarted = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterGroupsCleanupCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservationLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservationSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    RecurrenceStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecurrenceEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationSeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactListId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterGroups_ContactLists_ContactListId",
                        column: x => x.ContactListId,
                        principalTable: "ContactLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Join_CampaignRecipients",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Join_CampaignRecipients", x => new { x.CampaignId, x.RecipientId });
                    table.ForeignKey(
                        name: "FK_Join_CampaignRecipients_NewsletterGroupsCleanupCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "NewsletterGroupsCleanupCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Join_CampaignRecipients_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Join_ContactListRecipients",
                columns: table => new
                {
                    ContactListId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Join_ContactListRecipients", x => new { x.ContactListId, x.RecipientId });
                    table.ForeignKey(
                        name: "FK_Join_ContactListRecipients_ContactLists_ContactListId",
                        column: x => x.ContactListId,
                        principalTable: "ContactLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Join_ContactListRecipients_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Join_EmailRecipients",
                columns: table => new
                {
                    EmailId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Join_EmailRecipients", x => new { x.EmailId, x.RecipientId });
                    table.ForeignKey(
                        name: "FK_Join_EmailRecipients_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Join_EmailRecipients_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterSubscriptionConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmationExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscriptionConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterSubscriptionConfirmations_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterUnsubscribeConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmationExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterUnsubscribeConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterUnsubscribeConfirmations_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CommunityName = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    ReservationLocationId = table.Column<int>(type: "int", nullable: false),
                    ReservationSeriesId = table.Column<int>(type: "int", nullable: true),
                    ReservationHistoryId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_ReservationHistories_ReservationHistoryId",
                        column: x => x.ReservationHistoryId,
                        principalTable: "ReservationHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_ReservationLocations_ReservationLocationId",
                        column: x => x.ReservationLocationId,
                        principalTable: "ReservationLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_ReservationSeries_ReservationSeriesId",
                        column: x => x.ReservationSeriesId,
                        principalTable: "ReservationSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                columns: table => new
                {
                    NewsletterGroupsId = table.Column<int>(type: "int", nullable: false),
                    NewsletterSubscriptionConfirmationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Join_NewsletterSubscriptionConfirmationNewsletterGroups", x => new { x.NewsletterGroupsId, x.NewsletterSubscriptionConfirmationsId });
                    table.ForeignKey(
                        name: "FK_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                        column: x => x.NewsletterGroupsId,
                        principalTable: "NewsletterGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmations_NewsletterSubscriptionConfirmati~",
                        column: x => x.NewsletterSubscriptionConfirmationsId,
                        principalTable: "NewsletterSubscriptionConfirmations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                columns: table => new
                {
                    NewsletterGroupsId = table.Column<int>(type: "int", nullable: false),
                    NewsletterUnsubscribeConfirmationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups", x => new { x.NewsletterGroupsId, x.NewsletterUnsubscribeConfirmationsId });
                    table.ForeignKey(
                        name: "FK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                        column: x => x.NewsletterGroupsId,
                        principalTable: "NewsletterGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmations_NewsletterUnsubscribeConfirmations~",
                        column: x => x.NewsletterUnsubscribeConfirmationsId,
                        principalTable: "NewsletterUnsubscribeConfirmations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventCoordinatorId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    Uid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_EventCoordinators_EventCoordinatorId",
                        column: x => x.EventCoordinatorId,
                        principalTable: "EventCoordinators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BlobStorageFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileBaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlobStorageFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlobStorageFiles_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlobStorageFiles_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoverImageId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalBlobStorageFileId = table.Column<int>(type: "int", nullable: false),
                    DefaultBlobStorageFileId = table.Column<int>(type: "int", nullable: true),
                    ThumbnailBlobStorageFileId = table.Column<int>(type: "int", nullable: true),
                    DateTaken = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhotographerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Images_BlobStorageFiles_DefaultBlobStorageFileId",
                        column: x => x.DefaultBlobStorageFileId,
                        principalTable: "BlobStorageFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Images_BlobStorageFiles_OriginalBlobStorageFileId",
                        column: x => x.OriginalBlobStorageFileId,
                        principalTable: "BlobStorageFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Images_BlobStorageFiles_ThumbnailBlobStorageFileId",
                        column: x => x.ThumbnailBlobStorageFileId,
                        principalTable: "BlobStorageFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_CoverImageId",
                table: "Albums",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_DateCreated",
                table: "Albums",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Name",
                table: "Albums",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BlobStorageFiles_DateCreated",
                table: "BlobStorageFiles",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_BlobStorageFiles_EmailId",
                table: "BlobStorageFiles",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_BlobStorageFiles_EventId",
                table: "BlobStorageFiles",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedReservationTimeSlots_StartTime",
                table: "BlockedReservationTimeSlots",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_DateCreated",
                table: "Emails",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventCoordinatorId",
                table: "Events",
                column: "EventCoordinatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ReservationId",
                table: "Events",
                column: "ReservationId",
                unique: true,
                filter: "[ReservationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Images_AlbumId",
                table: "Images",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DateCreated",
                table: "Images",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DateTaken",
                table: "Images",
                column: "DateTaken");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images",
                column: "DefaultBlobStorageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_OriginalBlobStorageFileId",
                table: "Images",
                column: "OriginalBlobStorageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images",
                column: "ThumbnailBlobStorageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Join_CampaignRecipients_RecipientId",
                table: "Join_CampaignRecipients",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Join_ContactListRecipients_RecipientId",
                table: "Join_ContactListRecipients",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Join_EmailRecipients_RecipientId",
                table: "Join_EmailRecipients",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmationsId",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                column: "NewsletterSubscriptionConfirmationsId");

            migrationBuilder.CreateIndex(
                name: "IX_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmationsId",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                column: "NewsletterUnsubscribeConfirmationsId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterGroups_ContactListId",
                table: "NewsletterGroups",
                column: "ContactListId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterGroups_Name",
                table: "NewsletterGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterGroupsCleanupCampaigns_CampaignStart",
                table: "NewsletterGroupsCleanupCampaigns",
                column: "CampaignStart");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptionConfirmations_ConfirmationExpiry",
                table: "NewsletterSubscriptionConfirmations",
                column: "ConfirmationExpiry");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptionConfirmations_RecipientId",
                table: "NewsletterSubscriptionConfirmations",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterUnsubscribeConfirmations_ConfirmationExpiry",
                table: "NewsletterUnsubscribeConfirmations",
                column: "ConfirmationExpiry");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterUnsubscribeConfirmations_RecipientId",
                table: "NewsletterUnsubscribeConfirmations",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_EmailAddress",
                table: "Recipients",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_Token",
                table: "Recipients",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationHistories_Email",
                table: "ReservationHistories",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationHistories_Token",
                table: "ReservationHistories",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Email",
                table: "Reservations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationHistoryId",
                table: "Reservations",
                column: "ReservationHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationLocationId",
                table: "Reservations",
                column: "ReservationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationSeriesId",
                table: "Reservations",
                column: "ReservationSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeries_RecurrenceEndDate",
                table: "ReservationSeries",
                column: "RecurrenceEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeries_RecurrenceStartDate",
                table: "ReservationSeries",
                column: "RecurrenceStartDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Images_CoverImageId",
                table: "Albums",
                column: "CoverImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Images_CoverImageId",
                table: "Albums");

            migrationBuilder.DropTable(
                name: "BlockedReservationTimeSlots");

            migrationBuilder.DropTable(
                name: "ContactListUnsubscriptions");

            migrationBuilder.DropTable(
                name: "DailyContactListAnalytics");

            migrationBuilder.DropTable(
                name: "DailyEmailAnalytics");

            migrationBuilder.DropTable(
                name: "Join_CampaignRecipients");

            migrationBuilder.DropTable(
                name: "Join_ContactListRecipients");

            migrationBuilder.DropTable(
                name: "Join_EmailRecipients");

            migrationBuilder.DropTable(
                name: "Join_NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropTable(
                name: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropTable(
                name: "NewsletterGroupsCleanupCampaigns");

            migrationBuilder.DropTable(
                name: "NewsletterSubscriptionConfirmations");

            migrationBuilder.DropTable(
                name: "NewsletterGroups");

            migrationBuilder.DropTable(
                name: "NewsletterUnsubscribeConfirmations");

            migrationBuilder.DropTable(
                name: "ContactLists");

            migrationBuilder.DropTable(
                name: "Recipients");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "BlobStorageFiles");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EventCoordinators");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "ReservationHistories");

            migrationBuilder.DropTable(
                name: "ReservationLocations");

            migrationBuilder.DropTable(
                name: "ReservationSeries");
        }
    }
}
