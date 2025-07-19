// Test script to verify pin functionality fix
// Run this in browser console after logging in

console.log(' Testing Pin Functionality Fix');
console.log('================================');

// Test the corrected payload format
const testPinFix = async () => {
  console.log(' Step 1: Getting authentication token...');
  const token = localStorage.getItem('token');
  
  if (!token) {
    console.error(' No authentication token found. Please log in first.');
    return;
  }
  
  console.log(' Token found:', token.substring(0, 20) + '...');
  
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
    console.log('🎯 Using test note ID:', noteId);
    
    console.log(' Step 3: Testing pin API with NEW single-note format...');

    // NEW payload format - single note operation
    const correctPayload = {
      NoteId: parseInt(noteId),  // Convert to int as backend expects
      IsPinned: true
    };

    console.log('📤 Sending new single-note payload:', correctPayload);
    
    const pinResponse = await fetch('https://localhost:7256/api/Notes/pinUnpinNotes', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(correctPayload)
    });
    
    console.log('📥 Response status:', pinResponse.status);
    
    if (pinResponse.ok) {
      const result = await pinResponse.json();
      console.log('🎉 SUCCESS! Pin API is now working!');
      console.log(' Response:', result);
      
      // Test unpinning as well
      console.log(' Step 4: Testing unpin...');
      const unpinPayload = {
        NoteId: parseInt(noteId),
        IsPinned: false
      };
      
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
        console.log('🏆 CONCLUSION: Pin functionality is now FIXED!');
        console.log(' Updated to use single-note format: { NoteId: number, IsPinned: boolean }');
      } else {
        console.log(' Unpin failed, but pin worked');
      }
      
    } else {
      const errorText = await pinResponse.text();
      console.error(' Pin API still failing:', pinResponse.status);
      console.error('Error details:', errorText);
      console.log(' Check if backend expects different property names');
    }
    
  } catch (error) {
    console.error(' Test failed with error:', error);
  }
};

// Run the test
testPinFix();
