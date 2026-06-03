-- Migration: Add 'Delivered' to Orders.Status CHECK constraint
-- Required for: existing databases created before the Delivered status was added to OrderStatus enum
-- Safe to run multiple times (drops and recreates the constraint)
-- New databases created via EnsureCreated() already include this value automatically

DECLARE @constraintName NVARCHAR(128);
SELECT @constraintName = name
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('[orders].[Orders]')
  AND name LIKE '%Status%';

IF @constraintName IS NOT NULL
    EXEC('ALTER TABLE [orders].[Orders] DROP CONSTRAINT [' + @constraintName + ']');

ALTER TABLE [orders].[Orders]
ADD CONSTRAINT [CK_Orders_Status]
CHECK ([Status] IN ('Draft','Pending','Confirmed','Shipped','Delivered','Completed','Cancelled'));
