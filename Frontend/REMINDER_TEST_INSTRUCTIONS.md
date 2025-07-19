# Reminder Functionality Test Instructions

##  **How to Test the Reminder Functionality**

### Method 1: Browser Console Testing

1. **Open your application** in the browser
2. **Login** to your account
3. **Open Developer Tools** (F12)
4. **Go to Console tab**
5. **Create or find a note** and copy its ID
6. **Run this test command** in the console:

```javascript
// Option 1: Automatic test with first available note (RECOMMENDED)
// This will find the first note and test reminder functionality
// Open the console and paste this:
angular.element(document.body).injector().get('NoteService').getFirstNoteIdForTesting();

// Option 2: Manual test with specific note ID
// Replace 'your-note-id' with an actual note ID from your application
angular.element(document.body).injector().get('NoteService').testReminderFunctionality('your-note-id');
```

**Expected Output:**
```
 Testing reminder functionality for note: your-note-id
 Test 1: Adding reminder for tomorrow 8 AM
 Adding/updating reminder: {noteIdList: ["your-note-id"], reminder: "2024-12-31T08:00:00.000Z"}
 Test 1 PASSED: Reminder added successfully
 Test 2: Removing reminder
 Test 2 PASSED: Reminder removed successfully
🎉 All reminder tests PASSED!
```

### Method 2: Manual UI Testing

1. **Create a new note** or select an existing one
2. **Click the reminder icon** (bell icon) in the note card or dialog
3. **Select "Tomorrow"** from the dropdown
4. **Check the console** for success/error messages
5. **Verify the reminder appears** on the note card
6. **Go to Reminders page** to see if the note appears there
7. **Click the reminder icon again** and select "Remove reminder"
8. **Verify the reminder is removed** from the note

### Method 3: Network Tab Testing

1. **Open Developer Tools** (F12)
2. **Go to Network tab**
3. **Filter by "addUpdateReminderNotes"**
4. **Try to set a reminder** on a note
5. **Check the request details:**
   - Method: POST
   - URL: `https://localhost:7256/api/Notes/addUpdateReminderNotes`
   - Status: 200 (success) or error code
   - Request payload format
   - Response content

##  **What to Look For**

### Success Indicators:
-  Console shows "Add/update reminder response" with success message
-  Network request returns 200 status
-  Note card shows reminder time (e.g., "Tomorrow, 08:00")
-  Note appears in Reminders page
-  Reminder can be removed successfully

### Error Indicators:
-  Console shows error messages with status codes
-  Network request returns 400/401/404/500 status
-  No reminder time appears on note card
-  Note doesn't appear in Reminders page

## 🛠️ **Common Issues & Quick Fixes**

### Issue 1: "No authentication token found"
**Fix:** Make sure you're logged in. Check `localStorage.getItem('token')` in console.

### Issue 2: "Network error - Backend server may not be running"
**Fix:** Start your ASP.NET Core backend server on `https://localhost:7256`

### Issue 3: "400 Bad Request - Validation failed"
**Fix:** Check if the note ID exists and belongs to the current user.

### Issue 4: "404 Not Found - Check API endpoint URL"
**Fix:** Verify your backend has the `/api/Notes/addUpdateReminderNotes` endpoint.

##  **Backend Checklist**

Before testing, ensure your backend has:

1.  **BulkReminderDto** class with `NoteIdList` and `ReminderDateTime` properties
2.  **BulkRemoveReminderDto** class with `NoteIdList` property
3.  **Controller endpoints:**
   - `[HttpPost("addUpdateReminderNotes")]`
   - `[HttpPost("removeReminderNotes")]`
4.  **Authentication** middleware configured
5.  **CORS** configured for frontend domain
6.  **Database** with notes table and reminder column

## 🎯 **Expected Backend Behavior**

### For Add/Update Reminder:
1. Validate the request payload
2. Check if user owns the notes
3. Update the ReminderDateTime field in database
4. Return success response with updated notes

### For Remove Reminder:
1. Validate the request payload
2. Check if user owns the notes
3. Set ReminderDateTime to null in database
4. Return success response with updated notes

## 📞 **If You Need Help**

If the tests fail, please provide:

1. **Console error messages** (copy the full error)
2. **Network request details** (status, headers, payload, response)
3. **Your backend controller code** for the reminder endpoints
4. **Your BulkReminderDto class** definition
5. **Database schema** for the notes table

## 🚀 **Next Steps After Testing**

Once the basic functionality works:

1. **Test edge cases** (invalid dates, non-existent notes)
2. **Test bulk operations** (multiple notes at once)
3. **Test different reminder times** (today, next week, custom)
4. **Test the Reminders page** functionality
5. **Test reminder removal** from different UI locations

The frontend code is properly implemented and should work correctly with your backend. Most issues are typically related to backend configuration, authentication, or database setup.
