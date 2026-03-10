-- =============================================
-- Web POS System - SQL Server Agent Jobs
-- Automated Maintenance Job Scheduling
-- =============================================
-- Description: Creates SQL Server Agent jobs to run maintenance
--              procedures on a schedule
-- Prerequisites: SQL Server Agent must be running
--                User must have permissions to create jobs
-- =============================================

USE [msdb];
GO

PRINT '========================================';
PRINT 'Creating SQL Server Agent Jobs';
PRINT '========================================';
GO

-- =============================================
-- JOB 1: Cleanup Expired Order Locks
-- Schedule: Every 5 minutes
-- =============================================

-- Delete job if it already exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'WebPOS - Cleanup Expired Locks')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name = N'WebPOS - Cleanup Expired Locks';
    PRINT 'Deleted existing job: WebPOS - Cleanup Expired Locks';
END
GO

-- Create the job
EXEC msdb.dbo.sp_add_job
    @job_name = N'WebPOS - Cleanup Expired Locks',
    @enabled = 1,
    @description = N'Removes expired order locks from web.OrderLocks table every 5 minutes',
    @category_name = N'Database Maintenance',
    @owner_login_name = N'sa';
GO

-- Add job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = N'WebPOS - Cleanup Expired Locks',
    @step_name = N'Execute Cleanup Procedure',
    @subsystem = N'TSQL',
    @command = N'EXEC [POS].[web].[CleanupExpiredLocks];',
    @database_name = N'POS',
    @retry_attempts = 3,
    @retry_interval = 1;
GO

-- Create schedule (every 5 minutes)
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Every 5 Minutes',
    @enabled = 1,
    @freq_type = 4,  -- Daily
    @freq_interval = 1,
    @freq_subday_type = 4,  -- Minutes
    @freq_subday_interval = 5,
    @active_start_time = 000000;
GO

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'WebPOS - Cleanup Expired Locks',
    @schedule_name = N'Every 5 Minutes';
GO

-- Add job to local server
EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'WebPOS - Cleanup Expired Locks',
    @server_name = N'(local)';
GO

PRINT 'Created job: WebPOS - Cleanup Expired Locks (every 5 minutes)';
GO

-- =============================================
-- JOB 2: Cleanup Expired Sessions
-- Schedule: Every hour
-- =============================================

-- Delete job if it already exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'WebPOS - Cleanup Expired Sessions')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name = N'WebPOS - Cleanup Expired Sessions';
    PRINT 'Deleted existing job: WebPOS - Cleanup Expired Sessions';
END
GO

-- Create the job
EXEC msdb.dbo.sp_add_job
    @job_name = N'WebPOS - Cleanup Expired Sessions',
    @enabled = 1,
    @description = N'Marks expired user sessions as inactive every hour',
    @category_name = N'Database Maintenance',
    @owner_login_name = N'sa';
GO

-- Add job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = N'WebPOS - Cleanup Expired Sessions',
    @step_name = N'Execute Cleanup Procedure',
    @subsystem = N'TSQL',
    @command = N'EXEC [POS].[web].[CleanupExpiredSessions];',
    @database_name = N'POS',
    @retry_attempts = 3,
    @retry_interval = 5;
GO

-- Create schedule (every hour)
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Every Hour',
    @enabled = 1,
    @freq_type = 4,  -- Daily
    @freq_interval = 1,
    @freq_subday_type = 8,  -- Hours
    @freq_subday_interval = 1,
    @active_start_time = 000000;
GO

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'WebPOS - Cleanup Expired Sessions',
    @schedule_name = N'Every Hour';
GO

-- Add job to local server
EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'WebPOS - Cleanup Expired Sessions',
    @server_name = N'(local)';
GO

PRINT 'Created job: WebPOS - Cleanup Expired Sessions (every hour)';
GO

-- =============================================
-- JOB 3: Cleanup Old Audit Logs
-- Schedule: Daily at 2:00 AM
-- =============================================

-- Delete job if it already exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'WebPOS - Cleanup Old Audit Logs')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name = N'WebPOS - Cleanup Old Audit Logs';
    PRINT 'Deleted existing job: WebPOS - Cleanup Old Audit Logs';
END
GO

-- Create the job
EXEC msdb.dbo.sp_add_job
    @job_name = N'WebPOS - Cleanup Old Audit Logs',
    @enabled = 1,
    @description = N'Removes audit logs older than 90 days, runs daily at 2:00 AM',
    @category_name = N'Database Maintenance',
    @owner_login_name = N'sa';
GO

-- Add job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = N'WebPOS - Cleanup Old Audit Logs',
    @step_name = N'Execute Cleanup Procedure',
    @subsystem = N'TSQL',
    @command = N'EXEC [POS].[web].[CleanupOldAuditLogs] @RetentionDays = 90;',
    @database_name = N'POS',
    @retry_attempts = 3,
    @retry_interval = 10;
GO

-- Create schedule (daily at 2:00 AM)
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Daily at 2 AM',
    @enabled = 1,
    @freq_type = 4,  -- Daily
    @freq_interval = 1,
    @freq_subday_type = 1,  -- Once
    @active_start_time = 020000;  -- 2:00 AM
GO

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'WebPOS - Cleanup Old Audit Logs',
    @schedule_name = N'Daily at 2 AM';
GO

-- Add job to local server
EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'WebPOS - Cleanup Old Audit Logs',
    @server_name = N'(local)';
GO

PRINT 'Created job: WebPOS - Cleanup Old Audit Logs (daily at 2:00 AM)';
GO

-- =============================================
-- JOB 4: Cleanup Failed Sync Queue
-- Schedule: Daily at 3:00 AM
-- =============================================

-- Delete job if it already exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'WebPOS - Cleanup Failed Sync Queue')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name = N'WebPOS - Cleanup Failed Sync Queue';
    PRINT 'Deleted existing job: WebPOS - Cleanup Failed Sync Queue';
END
GO

-- Create the job
EXEC msdb.dbo.sp_add_job
    @job_name = N'WebPOS - Cleanup Failed Sync Queue',
    @enabled = 1,
    @description = N'Removes failed sync queue items older than 7 days, runs daily at 3:00 AM',
    @category_name = N'Database Maintenance',
    @owner_login_name = N'sa';
GO

-- Add job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = N'WebPOS - Cleanup Failed Sync Queue',
    @step_name = N'Execute Cleanup Procedure',
    @subsystem = N'TSQL',
    @command = N'EXEC [POS].[web].[CleanupFailedSyncQueue] @RetentionDays = 7, @MaxAttempts = 5;',
    @database_name = N'POS',
    @retry_attempts = 3,
    @retry_interval = 10;
GO

-- Create schedule (daily at 3:00 AM)
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Daily at 3 AM',
    @enabled = 1,
    @freq_type = 4,  -- Daily
    @freq_interval = 1,
    @freq_subday_type = 1,  -- Once
    @active_start_time = 030000;  -- 3:00 AM
GO

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'WebPOS - Cleanup Failed Sync Queue',
    @schedule_name = N'Daily at 3 AM';
GO

-- Add job to local server
EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'WebPOS - Cleanup Failed Sync Queue',
    @server_name = N'(local)';
GO

PRINT 'Created job: WebPOS - Cleanup Failed Sync Queue (daily at 3:00 AM)';
GO

-- =============================================
-- Verify Jobs Created
-- =============================================

PRINT '';
PRINT 'Verifying created jobs...';
GO

SELECT 
    j.name AS JobName,
    j.enabled AS IsEnabled,
    j.description AS Description,
    s.name AS ScheduleName,
    CASE s.freq_type
        WHEN 4 THEN 'Daily'
        WHEN 8 THEN 'Weekly'
        WHEN 16 THEN 'Monthly'
        ELSE 'Other'
    END AS Frequency,
    CASE s.freq_subday_type
        WHEN 1 THEN 'Once'
        WHEN 4 THEN 'Every ' + CAST(s.freq_subday_interval AS VARCHAR) + ' minutes'
        WHEN 8 THEN 'Every ' + CAST(s.freq_subday_interval AS VARCHAR) + ' hours'
        ELSE 'Other'
    END AS SubdayFrequency,
    STUFF(STUFF(RIGHT('000000' + CAST(s.active_start_time AS VARCHAR(6)), 6), 5, 0, ':'), 3, 0, ':') AS StartTime
FROM msdb.dbo.sysjobs j
LEFT JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
LEFT JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
WHERE j.name LIKE 'WebPOS -%'
ORDER BY j.name;
GO

PRINT '';
PRINT '========================================';
PRINT 'SQL Server Agent Jobs Created Successfully!';
PRINT '========================================';
PRINT '';
PRINT 'IMPORTANT NOTES:';
PRINT '1. Ensure SQL Server Agent service is running';
PRINT '2. Jobs will start automatically based on their schedules';
PRINT '3. Monitor job history in SQL Server Management Studio:';
PRINT '   SQL Server Agent > Jobs > [Job Name] > View History';
PRINT '';
PRINT '4. To manually run a job for testing:';
PRINT '   EXEC msdb.dbo.sp_start_job @job_name = ''WebPOS - Cleanup Expired Locks'';';
PRINT '';
PRINT '5. To disable a job:';
PRINT '   EXEC msdb.dbo.sp_update_job @job_name = ''WebPOS - Cleanup Expired Locks'', @enabled = 0;';
PRINT '';
PRINT '6. To modify retention periods, edit the job step command:';
PRINT '   EXEC msdb.dbo.sp_update_jobstep';
PRINT '       @job_name = ''WebPOS - Cleanup Old Audit Logs'',';
PRINT '       @step_id = 1,';
PRINT '       @command = ''EXEC [POS].[web].[CleanupOldAuditLogs] @RetentionDays = 180;'';';
PRINT '';
PRINT '7. To modify max sync attempts:';
PRINT '   EXEC msdb.dbo.sp_update_jobstep';
PRINT '       @job_name = ''WebPOS - Cleanup Failed Sync Queue'',';
PRINT '       @step_id = 1,';
PRINT '       @command = ''EXEC [POS].[web].[CleanupFailedSyncQueue] @RetentionDays = 7, @MaxAttempts = 10;'';';
PRINT '';
GO
