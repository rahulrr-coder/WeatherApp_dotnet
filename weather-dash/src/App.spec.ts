import { mount, flushPromises } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import App from './App.vue'

// ---------------------------------------------------------
// 1. SETUP THE MOCK (The "Fake Internet")
// ---------------------------------------------------------

// Create a "Spy" function that we can control
const fetchMock = vi.fn()

// Tell the test environment: "When the app calls 'fetch', use my spy instead."
vi.stubGlobal('fetch', fetchMock)

// Helper to make writing responses easier
function createFetchResponse(data: any) {
  return { 
    ok: true, 
    json: () => new Promise((resolve) => resolve(data)) 
  }
}

// ---------------------------------------------------------
// 2. THE TEST SUITE
// ---------------------------------------------------------
describe('Weather Dashboard', () => {
  
  // Runs before EVERY single test ('it' block)
  beforeEach(() => {
    // Clear history so Test A doesn't affect Test B
    vi.resetAllMocks()
    
    // Default Behavior: If we don't specify otherwise, return an empty array (for Favorites)
    fetchMock.mockResolvedValue(createFetchResponse([]))
  })

  // TEST 1: Sanity Check
  it('renders correctly', () => {
    const wrapper = mount(App)
    expect(wrapper.text()).toContain('Weather App')
  })

  // TEST 2: Search Functionality
  it('searches for weather and displays result', async () => {
    const wrapper = mount(App)
    
    // Setup: "When they search, give them Dubai weather"
    fetchMock.mockResolvedValueOnce(createFetchResponse({
      city: 'Dubai',
      temperature: 35,
      weather: 'Sunny',
      precipitation: 10
    }))

    // Action: Type "Dubai" and click Search
    await wrapper.find('input').setValue('Dubai')
    await wrapper.find('button').trigger('click')
    
    // Wait: Let the async API call finish
    await flushPromises()

    // Assert: Check if the screen updated
    expect(wrapper.text()).toContain('Dubai')
    expect(wrapper.text()).toContain('35°C')
    expect(wrapper.text()).toContain('Sunny')
  })

  // TEST 3: Favorites Feature
  it('shows save button when weather is loaded', async () => {
    const wrapper = mount(App)
    
    // Setup: "When they search, give them Paris weather"
    fetchMock.mockResolvedValueOnce(createFetchResponse({
      city: 'Paris',
      temperature: 20,
      weather: 'Cloudy',
      precipitation: 50
    }))

    // Action
    await wrapper.find('input').setValue('Paris')
    await wrapper.find('button').trigger('click')
    await flushPromises()

    // Assert: Check for the specific class '.save-btn'
    const saveBtn = wrapper.find('.save-btn')
    expect(saveBtn.exists()).toBe(true)
    expect(saveBtn.text()).toContain('Save to Favorites')
  })

  // TEST 4: Error Handling (The "Sad Path")
  it('displays error message when city is not found', async () => {
    const wrapper = mount(App)
    
    // Mock a broken API response (404)
    fetchMock.mockResolvedValueOnce({
      ok: false,
      json: async () => ({})
    })

    // Search for a fake city
    await wrapper.find('input').setValue('InvalidCity')
    await wrapper.find('button').trigger('click')
    await flushPromises()

    // Assert: The error message from App.vue ("City not found") is displayed
    expect(wrapper.text()).toContain('City not found')
  })

  // TEST 5: Verify Favorites Load on Startup
  it('loads favorites on startup', async () => {
    // Setup: Mock the initial GET request
    fetchMock.mockResolvedValueOnce(createFetchResponse([
      { id: 1, name: 'Mumbai' }
    ]))

    const wrapper = mount(App)
    await flushPromises() // Wait for onMounted

    // Assert: Mumbai should be visible in the list
    expect(wrapper.text()).toContain('Mumbai')
  })

  // TEST 6: Delete Functionality
  it('removes a city when delete button is clicked', async () => {
    // Setup: Start with one favorite
    fetchMock.mockResolvedValueOnce(createFetchResponse([
      { id: 99, name: 'Berlin' }
    ]))
    
    const wrapper = mount(App)
    await flushPromises()

    // Setup: Mock the DELETE request
    fetchMock.mockResolvedValueOnce(createFetchResponse({})) // DELETE success
    // Setup: Mock the Refresh request that happens after delete
    fetchMock.mockResolvedValueOnce(createFetchResponse([])) // Empty list back

    // Find and click the 'x' button
    const deleteBtn = wrapper.find('.delete-btn')
    await deleteBtn.trigger('click')
    await flushPromises()

    // Assert: API was called with DELETE
    const calls = fetchMock.mock.calls
    const deleteCall = calls.find((c: any[]) => c[0].includes('/Favorites/99') && c[1].method === 'DELETE')
    expect(deleteCall).toBeDefined()
  })
})