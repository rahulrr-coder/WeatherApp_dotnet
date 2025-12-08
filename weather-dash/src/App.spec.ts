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
    expect(wrapper.text()).toContain('Weather & Vue Connection')
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
})