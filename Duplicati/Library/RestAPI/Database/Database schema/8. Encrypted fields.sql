﻿/*
This update does nothing but the user cannot really downgrade,
because the fields that are encrypted cannot be read by the previous version.
*/
SELECT NULL FROM "Notification" LIMIT 1;

