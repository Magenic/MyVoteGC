﻿create view [Reports].[Polls] 
with schemabinding 
as 
SELECT [PollID] as [Poll Id]
      ,[UserID] as [User Id]
      ,[PollCategoryID] as [Category Id]
      ,[PollQuestion] as [Question]
      ,[PollDescription] as [Description]
      ,[PollImageLink] as [Image Link]
      ,[PollMaxAnswers] as [Max Answers]
      ,[PollMinAnswers] as [Min Answers]
      ,[PollStartDate] as [Start Date]
	  ,convert(int,convert(varchar,PollStartDate,112)) as [Start Date Id]
      ,[PollEndDate] as [End Date]
	  ,convert(int,convert(varchar,PollEndDate,112)) as [End Date Id]
      --,[PollAdminRemovedFlag]
      --,[PollDateRemoved]
      --,[PollDeletedFlag]
      --,[PollDeletedDate]
      ,[AuditDateCreated] as [Date Created]
	  ,convert(int,convert(varchar,AuditDateCreated,112)) as [Date Created Id]
      ,[AuditDateModified] as [Date Modified]
	  ,convert(int,convert(varchar,AuditDateModified,112)) as [Date Modified Id]
  FROM [dbo].[MVPoll] 
  where [PollDeletedFlag] = 0