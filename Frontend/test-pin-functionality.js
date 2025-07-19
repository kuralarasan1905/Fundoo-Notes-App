// Test Pin Functionality - Run this in browser console
// Copy and paste this entire script into your browser console to test pin functionality

console.log(' Starting Pin Functionality Test...');

// Test 1: Check if note service is available
if (typeof angular === 'undefined' && typeof ng === 'undefined') {
  console.log(' Testing in Angular application...');
  
  // Get the note service from Angular (if available)
  const testPinWithFirstNote = () => {
    console.log(' Looking for notes to test pin functionality...');
    
    // This will work if you have access to the Angular component
    // You can also manually test by clicking the pin buttons in the UI
    console.log(' Manual Test Instructions:');
    console.log('1. Look for notes in your application');
    console.log('2. Hover over a note to see the pin button');
    console.log('3. Click the pin button');
    console.log('4. Check the console for success/error messages');
    console.log('5. Verify the note moves to/from the PINNED section');
    
    console.log('🔧 Available debug methods:');
    console.log('- testPinFunctionality() - Test with first available note');
    console.log('- quickPinTest() - Quick pin test');
    console.log('- diagnosePinIssues() - Comprehensive diagnosis');
  };
  
  testPinWithFirstNote();
} else {
  console.log(' Angular context not detected');
}

// Test 2: Check authentication
const checkAuth = () => {
  const token = localStorage.getItem('token');
  console.log(' Authentication Check:');
  console.log('  Token exists:', !!token);
  console.log('  Token length:', token?.length || 0);
  
  if (!token) {
    console.error(' No authentication token found!');
    console.log(' Please log in first before testing pin functionality');
    return false;
  }
  
  console.log(' Authentication token found');
  return true;
};

// Test 3: Check backend connectivity
const testBackend = async () => {
  console.log(' Testing backend connectivity...');
  
  try {
    const response = await fetch('https://localhost:7256/api/Notes', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (response.ok) {
      console.log(' Backend is reachable and responding');
      return true;
    } else {
      console.error(' Backend responded with error:', response.status);
      return false;
    }
  } catch (error) {
    console.error(' Backend connectivity test failed:', error);
    console.log(' Make sure your ASP.NET Core backend is running on https://localhost:7256');
    return false;
  }
};

// Test 4: Test pin API directly
const testPinAPI = async (noteId) => {
  console.log(' Testing pin API directly...');
  
  const payload = {
    NoteIdList: [noteId],
    IsPinned: true
  };
  
  try {
    const response = await fetch('https://localhost:7256/api/Notes/pinUnpinNotes', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });
    
    if (response.ok) {
      const result = await response.json();
      console.log(' Pin API test successful:', result);
      return true;
    } else {
      console.error(' Pin API test failed:', response.status);
      const errorText = await response.text();
      console.error('Error details:', errorText);
      return false;
    }
  } catch (error) {
    console.error(' Pin API test error:', error);
    return false;
  }
};

// Main test function
const runPinTests = async () => {
  console.group(' PIN FUNCTIONALITY COMPREHENSIVE TEST');
  
  // Step 1: Check authentication
  if (!checkAuth()) {
    console.groupEnd();
    return;
  }
  
  // Step 2: Test backend connectivity
  const backendOk = await testBackend();
  if (!backendOk) {
    console.groupEnd();
    return;
  }
  
  // Step 3: Instructions for manual testing
  console.log(' MANUAL TESTING INSTRUCTIONS:');
  console.log('1. Go to your notes page');
  console.log('2. Hover over any note card');
  console.log('3. Click the pin button (push_pin icon)');
  console.log('4. Watch the console for API call logs');
  console.log('5. Verify the note moves to/from PINNED section');
  
  console.log(' WHAT TO LOOK FOR:');
  console.log(' " Pin/unpin request received:" - Component received click');
  console.log(' " Pin/Unpin API call:" - Service making API call');
  console.log(' " Pin/Unpin successful:" - Backend responded successfully');
  console.log(' " Local note updated:" - UI updated locally');
  
  console.log(' POSSIBLE ERRORS:');
  console.log(' "Invalid noteIdList" - Note ID validation failed');
  console.log(' "401 Unauthorized" - Authentication token expired');
  console.log(' "404 Not Found" - Backend endpoint not found');
  console.log(' "400 Bad Request" - Payload format incorrect');
  console.log(' "0 Network Error" - Backend server not running');
  
  console.groupEnd();
};

// Run the tests
runPinTests();

// Make test functions available globally
window.testPinFunctionality = runPinTests;
window.checkPinAuth = checkAuth;
window.testPinBackend = testBackend;

console.log('🎯 Pin functionality test completed!');
console.log(' Now try clicking pin buttons on your notes and watch the console');
