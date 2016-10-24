USE [master]
GO
/****** Object:  Database [REstate]    Script Date: 10/23/2016 8:53:31 PM ******/
CREATE DATABASE [REstate]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'REstate', FILENAME = N'C:\Users\OvanCrone\REstate.mdf' , SIZE = 7360KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'REstate_log', FILENAME = N'C:\Users\OvanCrone\REstate_log.ldf' , SIZE = 832KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [REstate] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [REstate].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [REstate] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [REstate] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [REstate] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [REstate] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [REstate] SET ARITHABORT OFF 
GO
ALTER DATABASE [REstate] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [REstate] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [REstate] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [REstate] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [REstate] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [REstate] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [REstate] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [REstate] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [REstate] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [REstate] SET  ENABLE_BROKER 
GO
ALTER DATABASE [REstate] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [REstate] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [REstate] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [REstate] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [REstate] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [REstate] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [REstate] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [REstate] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [REstate] SET  MULTI_USER 
GO
ALTER DATABASE [REstate] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [REstate] SET DB_CHAINING OFF 
GO
ALTER DATABASE [REstate] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [REstate] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [REstate] SET DELAYED_DURABILITY = DISABLED 
GO
USE [REstate]
GO
/****** Object:  Table [dbo].[ChronoTriggers]    Script Date: 10/23/2016 8:53:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ChronoTriggers](
	[ChronoTriggerId] [uniqueidentifier] NOT NULL,
	[MachineInstanceId] [uniqueidentifier] NOT NULL,
	[StateName] [varchar](255) NOT NULL,
	[TriggerName] [varchar](255) NOT NULL,
	[Payload] [varchar](4000) NULL,
	[FireAfter] [datetime] NOT NULL,
 CONSTRAINT [PK_ChronoTriggers] PRIMARY KEY CLUSTERED 
(
	[ChronoTriggerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Instances]    Script Date: 10/23/2016 8:53:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Instances](
	[InstanceId] [varchar](250) NOT NULL,
	[MachineName] [varchar](250) NOT NULL,
	[StateName] [varchar](250) NOT NULL,
	[CommitTag] [varchar](250) NOT NULL,
	[StateChangedDateTime] [datetime] NOT NULL,
	[TriggerName] [varchar](250) NULL,
	[Metadata] [varchar](4000) NULL,
	[ParameterData] [varchar](4000) NULL,
 CONSTRAINT [PK_Instances] PRIMARY KEY CLUSTERED 
(
	[InstanceId] ASC,
	[StateName] ASC,
	[CommitTag] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Machines]    Script Date: 10/23/2016 8:53:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Machines](
	[MachineName] [varchar](250) NOT NULL,
	[ForkedFrom] [varchar](250) NULL,
	[InitialState] [varchar](250) NOT NULL,
	[AutoIgnoreTriggers] [bit] NOT NULL CONSTRAINT [DF_Machines_AutoIgnoreTriggers]  DEFAULT ((0)),
	[Definition] [varchar](max) NOT NULL,
	[CreatedDateTime] [datetime] NOT NULL CONSTRAINT [DF_Machines_CreatedDateTime]  DEFAULT (getutcdate()),
 CONSTRAINT [PK_Machines] PRIMARY KEY CLUSTERED 
(
	[MachineName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PrincipalClaims]    Script Date: 10/23/2016 8:53:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PrincipalClaims](
	[ApiKey] [uniqueidentifier] NOT NULL,
	[ClaimName] [varchar](255) NOT NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_PrincipalClaims_IsActive]  DEFAULT ((1)),
 CONSTRAINT [PK_PrincipalClaims] PRIMARY KEY CLUSTERED 
(
	[ApiKey] ASC,
	[ClaimName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Principals]    Script Date: 10/23/2016 8:53:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Principals](
	[ApiKey] [uniqueidentifier] NOT NULL,
	[PrincipalType] [varchar](50) NOT NULL,
	[UserOrApplicationName] [varchar](255) NOT NULL,
	[PasswordHash] [varchar](1000) NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_Principals_IsActive]  DEFAULT ((1)),
 CONSTRAINT [PK_Principals] PRIMARY KEY CLUSTERED 
(
	[ApiKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Instances] ADD  CONSTRAINT [DF_Instances_CommitTag]  DEFAULT (newid()) FOR [CommitTag]
GO
ALTER TABLE [dbo].[Instances] ADD  CONSTRAINT [DF_Instances_StateChangedDateTime]  DEFAULT (getutcdate()) FOR [StateChangedDateTime]
GO
ALTER TABLE [dbo].[Machines]  WITH CHECK ADD  CONSTRAINT [FK_Machines_ForkedFrom_Machines_MachineName] FOREIGN KEY([ForkedFrom])
REFERENCES [dbo].[Machines] ([MachineName])
GO
ALTER TABLE [dbo].[Machines] CHECK CONSTRAINT [FK_Machines_ForkedFrom_Machines_MachineName]
GO
ALTER TABLE [dbo].[PrincipalClaims]  WITH CHECK ADD  CONSTRAINT [FK_PrincipalClaims_Principals] FOREIGN KEY([ApiKey])
REFERENCES [dbo].[Principals] ([ApiKey])
GO
ALTER TABLE [dbo].[PrincipalClaims] CHECK CONSTRAINT [FK_PrincipalClaims_Principals]
GO
USE [master]
GO
ALTER DATABASE [REstate] SET  READ_WRITE 
GO
