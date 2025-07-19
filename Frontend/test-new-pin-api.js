// Test script for NEW single-note pin API format
// Run this in browser console after logging in

console.log(' Testing NEW Pin API Format');
console.log('==============================');
console.log('Backend expects: { NoteId: number, IsPinned: boolean }');

const testNewPinAPI = async () => {
  console.log(' Step 1: Getting authentication token...');
  const token = localStorage.getItem('token');
  
  if (!token) {
    console.error(' No authentication token found. Please log in first.');
    return;
  }
  
  console.log(' Token found');
  
  console.log(' Step 2: Getting notes list...');
  try {
    const notesResponse = await fetch('https://localhost:7256/api/Notes', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (!notesResponse.ok) {
      console.error(' Failed to get notes:', notesResponse.status);
      return;
    }
    
    const notes = await notesResponse.json();
    console.log(' Notes retrieved:', notes.length, 'notes found');
    
    if (notes.length === 0) {
      console.log(' No notes available for testing. Create a note first.');
      return;
    }
    
    const testNote = notes[0];
    const noteId = testNote.id || testNote.noteId || testNote.Id;
    console.log('🎯 Using test note ID:', noteId, '(type:', typeof noteId, ')');
    
    // Test the NEW single-note API format
    console.log(' Step 3: Testing NEW single-note pin API...');
    
    const newPayload = {
      NoteId: parseInt(noteId),  // Convert to int as backend expects
      IsPinned: true
    };
    
    console.log('📤 Sending NEW payload format:', newPayload);
    
    const pinResponse = await fetch('https://localhost:7256/api/Notes/pinUnpinNotes', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(newPayload)
    });
    
    console.log('📥 Response status:', pinResponse.status);
    
    if (pinResponse.ok) {
      const result = await pinResponse.json();
      console.log('🎉 SUCCESS! NEW Pin API format works!');
      console.log(' Pin response:', result);
      
      // Test unpinning
      console.log(' Step 4: Testing unpin with NEW format...');
      const unpinPayload = {
        NoteId: parseInt(noteId),
        IsPinned: false
      };
      
      console.log('📤 Sending unpin payload:', unpinPayload);
      
      const unpinResponse = await fetch('https://localhost:7256/api/Notes/pinUnpinNotes', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(unpinPayload)
      });
      
      if (unpinResponse.ok) {
        const unpinResult = await unpinResponse.json();
        console.log('🎉 UNPIN ALSO WORKS!');
        console.log(' Unpin response:', unpinResult);
        console.log('');
        console.log('🏆 CONCLUSION: NEW Pin API format is working perfectly!');
        console.log(' Frontend updated to use: { NoteId: number, IsPinned: boolean }');
        console.log(' Backend accepts single-note operations');
      } else {
        const unpinError = await unpinResponse.text();
        console.log(' Unpin failed:', unpinResponse.status);
        console.log('Error:', unpinError);
      }
      
    } else {
      const errorText = await pinResponse.text();
      console.error(' NEW Pin API format failed:', pinResponse.status);
      console.error('Error details:', errorText);
      
      // Provide specific guidance based on error
      if (pinResponse.status === 400) {
        console.log(' Bad Request - Check payload format');
        console.log(' Expected: { NoteId: number, IsPinned: boolean }');
        console.log(' Sent:', newPayload);
      } else if (pinResponse.status === 404) {
        console.log(' Note not found - Check if note ID exists');
      } else if (pinResponse.status === 401) {
        console.log(' Unauthorized - Check authentication');
      }
    }
    
  } catch (error) {
    console.error(' Test failed with error:', error);
  }
};

// Run the test
testNewPinAPI();
