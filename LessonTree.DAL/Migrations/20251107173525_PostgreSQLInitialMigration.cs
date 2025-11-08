using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PostgreSQLInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aspnetroles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedname = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrencystamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetroles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    filename = table.Column<string>(type: "text", nullable: false),
                    filesize = table.Column<int>(type: "integer", nullable: false),
                    contenttype = table.Column<string>(type: "text", nullable: true),
                    blob = table.Column<byte[]>(type: "bytea", nullable: false),
                    googledocurl = table.Column<string>(type: "text", nullable: true),
                    googledocid = table.Column<string>(type: "text", nullable: true),
                    shared = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "districts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_districts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "systemconfigs",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updateddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systemconfigs", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "aspnetroleclaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    roleid = table.Column<int>(type: "integer", nullable: false),
                    claimtype = table.Column<string>(type: "text", nullable: true),
                    claimvalue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetroleclaims", x => x.id);
                    table.ForeignKey(
                        name: "FK_aspnetroleclaims_aspnetroles_roleid",
                        column: x => x.roleid,
                        principalTable: "aspnetroles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schools",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    districtid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schools", x => x.id);
                    table.ForeignKey(
                        name: "FK_schools_districts_districtid",
                        column: x => x.districtid,
                        principalTable: "districts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetusers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    firstname = table.Column<string>(type: "text", nullable: false),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    districtid = table.Column<int>(type: "integer", nullable: true),
                    schoolid = table.Column<int>(type: "integer", nullable: true),
                    username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedusername = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedemail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    emailconfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    passwordhash = table.Column<string>(type: "text", nullable: true),
                    securitystamp = table.Column<string>(type: "text", nullable: true),
                    concurrencystamp = table.Column<string>(type: "text", nullable: true),
                    phonenumber = table.Column<string>(type: "text", nullable: true),
                    phonenumberconfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    twofactorenabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockoutend = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockoutenabled = table.Column<bool>(type: "boolean", nullable: false),
                    accessfailedcount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetusers", x => x.id);
                    table.ForeignKey(
                        name: "FK_aspnetusers_districts_districtid",
                        column: x => x.districtid,
                        principalTable: "districts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_aspnetusers_schools_schoolid",
                        column: x => x.schoolid,
                        principalTable: "schools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    schoolid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                    table.ForeignKey(
                        name: "FK_departments_schools_schoolid",
                        column: x => x.schoolid,
                        principalTable: "schools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetuserclaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    claimtype = table.Column<string>(type: "text", nullable: true),
                    claimvalue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserclaims", x => x.id);
                    table.ForeignKey(
                        name: "FK_aspnetuserclaims_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetuserlogins",
                columns: table => new
                {
                    loginprovider = table.Column<string>(type: "text", nullable: false),
                    providerkey = table.Column<string>(type: "text", nullable: false),
                    providerdisplayname = table.Column<string>(type: "text", nullable: true),
                    userid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserlogins", x => new { x.loginprovider, x.providerkey });
                    table.ForeignKey(
                        name: "FK_aspnetuserlogins_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetuserroles",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false),
                    roleid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserroles", x => new { x.userid, x.roleid });
                    table.ForeignKey(
                        name: "FK_aspnetuserroles_aspnetroles_roleid",
                        column: x => x.roleid,
                        principalTable: "aspnetroles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aspnetuserroles_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetusertokens",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false),
                    loginprovider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetusertokens", x => new { x.userid, x.loginprovider, x.name });
                    table.ForeignKey(
                        name: "FK_aspnetusertokens_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                    table.ForeignKey(
                        name: "FK_courses_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduleconfigurations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    schoolyear = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    startdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    enddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodsperday = table.Column<int>(type: "integer", nullable: false),
                    teachingdays = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lastupdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    archiveddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    istemplate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduleconfigurations", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduleconfigurations_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userconfigurations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    lastupdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    settingsjson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userconfigurations", x => x.id);
                    table.ForeignKey(
                        name: "FK_userconfigurations_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userdepartments",
                columns: table => new
                {
                    departmentsid = table.Column<int>(type: "integer", nullable: false),
                    membersid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userdepartments", x => new { x.departmentsid, x.membersid });
                    table.ForeignKey(
                        name: "FK_userdepartments_aspnetusers_membersid",
                        column: x => x.membersid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userdepartments_departments_departmentsid",
                        column: x => x.departmentsid,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    courseid = table.Column<int>(type: "integer", nullable: false),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topics", x => x.id);
                    table.ForeignKey(
                        name: "FK_topics_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_topics_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "periodassignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scheduleconfigurationid = table.Column<int>(type: "integer", nullable: false),
                    period = table.Column<int>(type: "integer", nullable: false),
                    courseid = table.Column<int>(type: "integer", nullable: true),
                    specialperiodtype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    teachingdays = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    room = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    backgroundcolor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    fontcolor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodassignments", x => x.id);
                    table.CheckConstraint("CK_PeriodAssignment_CourseId_Positive", "courseid IS NULL OR courseid > 0");
                    table.CheckConstraint("CK_PeriodAssignment_ExclusiveAssignment", "(courseid IS NOT NULL AND specialperiodtype IS NULL) OR (courseid IS NULL AND specialperiodtype IS NOT NULL)");
                    table.CheckConstraint("CK_PeriodAssignment_TeachingDays_NotEmpty", "teachingdays IS NOT NULL AND LENGTH(TRIM(teachingdays)) > 0");
                    table.CheckConstraint("CK_PeriodAssignment_TeachingDays_ValidDays", "teachingdays NOT LIKE '%[^MondayTueswdhFrig,]%' AND\n                        (teachingdays LIKE '%Monday%' OR\n                        teachingdays LIKE '%Tuesday%' OR\n                        teachingdays LIKE '%Wednesday%' OR\n                        teachingdays LIKE '%Thursday%' OR\n                        teachingdays LIKE '%Friday%' OR\n                        teachingdays LIKE '%Saturday%' OR\n                        teachingdays LIKE '%Sunday%')");
                    table.ForeignKey(
                        name: "FK_periodassignments_scheduleconfigurations_scheduleconfigurat~",
                        column: x => x.scheduleconfigurationid,
                        principalTable: "scheduleconfigurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    scheduleconfigurationid = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    islocked = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    courseid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_schedules_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_schedules_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_schedules_scheduleconfigurations_scheduleconfigurationid",
                        column: x => x.scheduleconfigurationid,
                        principalTable: "scheduleconfigurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "standards",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    courseid = table.Column<int>(type: "integer", nullable: false),
                    topicid = table.Column<int>(type: "integer", nullable: true),
                    districtid = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    standardtype = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_standards", x => x.id);
                    table.ForeignKey(
                        name: "FK_standards_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_standards_districts_districtid",
                        column: x => x.districtid,
                        principalTable: "districts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_standards_topics_topicid",
                        column: x => x.topicid,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "subtopics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    topicid = table.Column<int>(type: "integer", nullable: false),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    isdefault = table.Column<bool>(type: "boolean", nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtopics", x => x.id);
                    table.ForeignKey(
                        name: "FK_subtopics_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subtopics_topics_topicid",
                        column: x => x.topicid,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "specialdays",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scheduleid = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periods = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    eventtype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    backgroundcolor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    fontcolor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specialdays", x => x.id);
                    table.ForeignKey(
                        name: "FK_specialdays_schedules_scheduleid",
                        column: x => x.scheduleid,
                        principalTable: "schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    objective = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    level = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    materials = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    classtime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    methods = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    specialneeds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    assessment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    subtopicid = table.Column<int>(type: "integer", nullable: true),
                    topicid = table.Column<int>(type: "integer", nullable: true),
                    archived = table.Column<bool>(type: "boolean", nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessons", x => x.id);
                    table.ForeignKey(
                        name: "FK_lessons_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lessons_subtopics_subtopicid",
                        column: x => x.subtopicid,
                        principalTable: "subtopics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lessons_topics_topicid",
                        column: x => x.topicid,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessonattachments",
                columns: table => new
                {
                    lessonid = table.Column<int>(type: "integer", nullable: false),
                    attachmentid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessonattachments", x => new { x.lessonid, x.attachmentid });
                    table.ForeignKey(
                        name: "FK_lessonattachments_attachments_attachmentid",
                        column: x => x.attachmentid,
                        principalTable: "attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lessonattachments_lessons_lessonid",
                        column: x => x.lessonid,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessonstandards",
                columns: table => new
                {
                    lessonid = table.Column<int>(type: "integer", nullable: false),
                    standardid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessonstandards", x => new { x.lessonid, x.standardid });
                    table.ForeignKey(
                        name: "FK_lessonstandards_lessons_lessonid",
                        column: x => x.lessonid,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lessonstandards_standards_standardid",
                        column: x => x.standardid,
                        principalTable: "standards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content = table.Column<string>(type: "text", nullable: false),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    courseid = table.Column<int>(type: "integer", nullable: true),
                    topicid = table.Column<int>(type: "integer", nullable: true),
                    subtopicid = table.Column<int>(type: "integer", nullable: true),
                    lessonid = table.Column<int>(type: "integer", nullable: true),
                    visibility = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_notes_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notes_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notes_lessons_lessonid",
                        column: x => x.lessonid,
                        principalTable: "lessons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notes_subtopics_subtopicid",
                        column: x => x.subtopicid,
                        principalTable: "subtopics",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notes_topics_topicid",
                        column: x => x.topicid,
                        principalTable: "topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "scheduleevents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scheduleid = table.Column<int>(type: "integer", nullable: false),
                    courseid = table.Column<int>(type: "integer", nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period = table.Column<int>(type: "integer", nullable: false),
                    lessonid = table.Column<int>(type: "integer", nullable: true),
                    specialdayid = table.Column<int>(type: "integer", nullable: true),
                    eventtype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    eventcategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    schedulesort = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduleevents", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduleevents_lessons_lessonid",
                        column: x => x.lessonid,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_scheduleevents_schedules_scheduleid",
                        column: x => x.scheduleid,
                        principalTable: "schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scheduleevents_specialdays_specialdayid",
                        column: x => x.specialdayid,
                        principalTable: "specialdays",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aspnetroleclaims_roleid",
                table: "aspnetroleclaims",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "aspnetroles",
                column: "normalizedname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserclaims_userid",
                table: "aspnetuserclaims",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserlogins_userid",
                table: "aspnetuserlogins",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserroles_roleid",
                table: "aspnetuserroles",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "aspnetusers",
                column: "normalizedemail");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetusers_districtid",
                table: "aspnetusers",
                column: "districtid");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetusers_schoolid",
                table: "aspnetusers",
                column: "schoolid");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "aspnetusers",
                column: "normalizedusername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_courses_userid",
                table: "courses",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_departments_schoolid",
                table: "departments",
                column: "schoolid");

            migrationBuilder.CreateIndex(
                name: "IX_lessonattachments_attachmentid",
                table: "lessonattachments",
                column: "attachmentid");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_Container_SortOrder",
                table: "lessons",
                columns: new[] { "topicid", "subtopicid", "sortorder" });

            migrationBuilder.CreateIndex(
                name: "IX_lessons_subtopicid",
                table: "lessons",
                column: "subtopicid");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_UserId_TopicId_SubTopicId",
                table: "lessons",
                columns: new[] { "userid", "topicid", "subtopicid" });

            migrationBuilder.CreateIndex(
                name: "IX_lessonstandards_standardid",
                table: "lessonstandards",
                column: "standardid");

            migrationBuilder.CreateIndex(
                name: "IX_notes_courseid",
                table: "notes",
                column: "courseid");

            migrationBuilder.CreateIndex(
                name: "IX_notes_lessonid",
                table: "notes",
                column: "lessonid");

            migrationBuilder.CreateIndex(
                name: "IX_notes_subtopicid",
                table: "notes",
                column: "subtopicid");

            migrationBuilder.CreateIndex(
                name: "IX_notes_topicid",
                table: "notes",
                column: "topicid");

            migrationBuilder.CreateIndex(
                name: "IX_notes_userid",
                table: "notes",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_periodassignments_scheduleconfigurationid_period",
                table: "periodassignments",
                columns: new[] { "scheduleconfigurationid", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scheduleconfigurations_userid_schoolyear",
                table: "scheduleconfigurations",
                columns: new[] { "userid", "schoolyear" });

            migrationBuilder.CreateIndex(
                name: "IX_scheduleconfigurations_userid_status",
                table: "scheduleconfigurations",
                columns: new[] { "userid", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEvents_LessonId",
                table: "scheduleevents",
                column: "lessonid");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEvents_Schedule_Date_Period",
                table: "scheduleevents",
                columns: new[] { "scheduleid", "date", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEvents_Schedule_Lesson",
                table: "scheduleevents",
                columns: new[] { "scheduleid", "lessonid" });

            migrationBuilder.CreateIndex(
                name: "IX_scheduleevents_specialdayid",
                table: "scheduleevents",
                column: "specialdayid");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_courseid",
                table: "schedules",
                column: "courseid");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_scheduleconfigurationid",
                table: "schedules",
                column: "scheduleconfigurationid");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_userid",
                table: "schedules",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_schools_districtid",
                table: "schools",
                column: "districtid");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialDays_Schedule_Date",
                table: "specialdays",
                columns: new[] { "scheduleid", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_standards_courseid",
                table: "standards",
                column: "courseid");

            migrationBuilder.CreateIndex(
                name: "IX_standards_districtid",
                table: "standards",
                column: "districtid");

            migrationBuilder.CreateIndex(
                name: "IX_standards_topicid",
                table: "standards",
                column: "topicid");

            migrationBuilder.CreateIndex(
                name: "IX_SubTopics_Topic_SortOrder",
                table: "subtopics",
                columns: new[] { "topicid", "sortorder" });

            migrationBuilder.CreateIndex(
                name: "IX_subtopics_userid",
                table: "subtopics",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Course_SortOrder",
                table: "topics",
                columns: new[] { "courseid", "sortorder" });

            migrationBuilder.CreateIndex(
                name: "IX_topics_userid",
                table: "topics",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_userconfigurations_userid",
                table: "userconfigurations",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_userdepartments_membersid",
                table: "userdepartments",
                column: "membersid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aspnetroleclaims");

            migrationBuilder.DropTable(
                name: "aspnetuserclaims");

            migrationBuilder.DropTable(
                name: "aspnetuserlogins");

            migrationBuilder.DropTable(
                name: "aspnetuserroles");

            migrationBuilder.DropTable(
                name: "aspnetusertokens");

            migrationBuilder.DropTable(
                name: "lessonattachments");

            migrationBuilder.DropTable(
                name: "lessonstandards");

            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "periodassignments");

            migrationBuilder.DropTable(
                name: "scheduleevents");

            migrationBuilder.DropTable(
                name: "systemconfigs");

            migrationBuilder.DropTable(
                name: "userconfigurations");

            migrationBuilder.DropTable(
                name: "userdepartments");

            migrationBuilder.DropTable(
                name: "aspnetroles");

            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "standards");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "specialdays");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "subtopics");

            migrationBuilder.DropTable(
                name: "schedules");

            migrationBuilder.DropTable(
                name: "topics");

            migrationBuilder.DropTable(
                name: "scheduleconfigurations");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "aspnetusers");

            migrationBuilder.DropTable(
                name: "schools");

            migrationBuilder.DropTable(
                name: "districts");
        }
    }
}
