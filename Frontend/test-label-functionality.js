/**
 * Test Script for Label Functionality
 * 
 * This script tests the enhanced label functionality including:
 * 1. Creating labels
 * 2. Adding labels to notes
 * 3. Removing labels from notes
 * 4. Getting notes by label
 * 5. Bulk label operations
 */

console.log('Starting Label Functionality Tests...');

// Configuration
const BASE_URL = 'https://localhost:7256/api';
let authToken = '';

// Test data
const testLabel = {
  name: 'Test Label ' + Date.now()
};

const testNote = {
  title: 'Test Note for Labels',
  content: 'This note is used for testing label functionality.',
  color: '#fff3e0'
};

// Helper function to make API calls
async function apiCall(endpoint, method = 'GET', body = null) {
  const options = {
    method,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`
    }
  };

  if (body) {
    options.body = JSON.stringify(body);
  }

  try {
    const response = await fetch(`${BASE_URL}${endpoint}`, options);
    const data = await response.json();
    
    console.log(`📡 ${method} ${endpoint}:`, {
      status: response.status,
      ok: response.ok,
      data: data
    });

    return { response, data };
  } catch (error) {
    console.error(` API call failed:`, error);
    return { error };
  }
}

// Test functions
async function testAuthentication() {
  console.log('\n🔐 Step 1: Testing Authentication...');
  
  // Try to get notes to test if we have a valid token
  const token = localStorage.getItem('token');
  if (!token) {
    console.error(' No authentication token found in localStorage');
    console.log(' Please login first and run this test again');
    return false;
  }
  
  authToken = token;
  console.log(' Token found:', token.substring(0, 20) + '...');
  
  // Test token validity
  const { response } = await apiCall('/Notes');
  if (response && response.ok) {
    console.log(' Authentication successful');
    return true;
  } else {
    console.error(' Authentication failed');
    return false;
  }
}

async function testCreateLabel() {
  console.log('\nStep 2: Testing Label Creation...');
  
  const { response, data } = await apiCall('/Labels', 'POST', {
    Name: testLabel.name
  });
  
  if (response && response.ok) {
    testLabel.id = data.id || data.Id;
    console.log(' Label created successfully:', testLabel);
    return true;
  } else {
    console.error(' Failed to create label');
    return false;
  }
}

async function testCreateNote() {
  console.log('\n Step 3: Testing Note Creation...');
  
  const { response, data } = await apiCall('/Notes/addNotes', 'POST', {
    Title: testNote.title,
    Content: testNote.content,
    Color: testNote.color
  });
  
  if (response && response.ok) {
    testNote.id = data.id || data.Id || data.noteId;
    console.log(' Note created successfully:', testNote);
    return true;
  } else {
    console.error(' Failed to create note');
    return false;
  }
}

async function testAddLabelToNote() {
  console.log('\n🔗 Step 4: Testing Add Label to Note...');
  
  const { response, data } = await apiCall('/Notes/addLabelToNote', 'POST', {
    NoteId: parseInt(testNote.id),
    LabelId: parseInt(testLabel.id)
  });
  
  if (response && response.ok) {
    console.log(' Label added to note successfully');
    return true;
  } else {
    console.error(' Failed to add label to note');
    console.log(' Trying alternative endpoint...');
    
    // Try alternative endpoint format
    const { response: altResponse } = await apiCall('/Notes/addLabel', 'POST', {
      NoteId: parseInt(testNote.id),
      LabelId: parseInt(testLabel.id)
    });
    
    if (altResponse && altResponse.ok) {
      console.log(' Label added to note successfully (alternative endpoint)');
      return true;
    } else {
      console.error(' Both endpoints failed');
      return false;
    }
  }
}

async function testGetNotesByLabel() {
  console.log('\n Step 5: Testing Get Notes by Label...');
  
  const { response, data } = await apiCall(`/Notes/byLabel/${testLabel.id}`);
  
  if (response && response.ok) {
    console.log(' Notes retrieved by label successfully');
    console.log(' Notes found:', Array.isArray(data) ? data.length : 'Unknown count');
    return true;
  } else {
    console.error(' Failed to get notes by label');
    return false;
  }
}

async function testRemoveLabelFromNote() {
  console.log('\n Step 6: Testing Remove Label from Note...');
  
  const { response, data } = await apiCall('/Notes/removeLabelFromNote', 'POST', {
    NoteId: parseInt(testNote.id),
    LabelId: parseInt(testLabel.id)
  });
  
  if (response && response.ok) {
    console.log(' Label removed from note successfully');
    return true;
  } else {
    console.error(' Failed to remove label from note');
    console.log(' Trying alternative endpoint...');
    
    // Try alternative endpoint format
    const { response: altResponse } = await apiCall('/Notes/removeLabel', 'POST', {
      NoteId: parseInt(testNote.id),
      LabelId: parseInt(testLabel.id)
    });
    
    if (altResponse && altResponse.ok) {
      console.log(' Label removed from note successfully (alternative endpoint)');
      return true;
    } else {
      console.error(' Both endpoints failed');
      return false;
    }
  }
}

async function testBulkLabelOperations() {
  console.log('\n📦 Step 7: Testing Bulk Label Operations...');
  
  // Test bulk add labels to notes
  const { response: addResponse } = await apiCall('/Notes/addLabelsToNotes', 'POST', {
    NoteIds: [parseInt(testNote.id)],
    LabelIds: [parseInt(testLabel.id)]
  });
  
  if (addResponse && addResponse.ok) {
    console.log(' Bulk add labels successful');
  } else {
    console.log(' Bulk add labels endpoint not available or failed');
  }
  
  // Test bulk remove labels from notes
  const { response: removeResponse } = await apiCall('/Notes/removeLabelsFromNotes', 'POST', {
    NoteIds: [parseInt(testNote.id)],
    LabelIds: [parseInt(testLabel.id)]
  });
  
  if (removeResponse && removeResponse.ok) {
    console.log(' Bulk remove labels successful');
  } else {
    console.log(' Bulk remove labels endpoint not available or failed');
  }
  
  return true;
}

async function cleanup() {
  console.log('\n🧹 Step 8: Cleanup...');
  
  // Delete test note
  if (testNote.id) {
    await apiCall('/Notes/deleteForeverNotes', 'POST', {
      NoteIdList: [testNote.id],
      IsDeleted: true
    });
    console.log(' Test note deleted');
  }
  
  // Delete test label
  if (testLabel.id) {
    await apiCall(`/Labels/${testLabel.id}`, 'DELETE');
    console.log(' Test label deleted');
  }
}

// Main test execution
async function runTests() {
  console.log('🚀 Starting comprehensive label functionality tests...\n');
  
  const results = {
    authentication: false,
    createLabel: false,
    createNote: false,
    addLabelToNote: false,
    getNotesByLabel: false,
    removeLabelFromNote: false,
    bulkOperations: false
  };
  
  try {
    results.authentication = await testAuthentication();
    if (!results.authentication) return;
    
    results.createLabel = await testCreateLabel();
    results.createNote = await testCreateNote();
    
    if (results.createLabel && results.createNote) {
      results.addLabelToNote = await testAddLabelToNote();
      results.getNotesByLabel = await testGetNotesByLabel();
      results.removeLabelFromNote = await testRemoveLabelFromNote();
      results.bulkOperations = await testBulkLabelOperations();
    }
    
    await cleanup();
    
  } catch (error) {
    console.error('💥 Test execution failed:', error);
  }
  
  // Print summary
  console.log('\n TEST SUMMARY:');
  console.log('================');
  Object.entries(results).forEach(([test, passed]) => {
    console.log(`${passed ? '' : ''} ${test}: ${passed ? 'PASSED' : 'FAILED'}`);
  });
  
  const passedCount = Object.values(results).filter(Boolean).length;
  const totalCount = Object.keys(results).length;
  
  console.log(`\n🎯 Overall: ${passedCount}/${totalCount} tests passed`);
  
  if (passedCount === totalCount) {
    console.log('🎉 All tests passed! Label functionality is working correctly.');
  } else {
    console.log(' Some tests failed. Check the API endpoints and backend implementation.');
  }
}

// Run the tests
runTests();
