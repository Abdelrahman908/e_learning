namespace e_learning.Service
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;

        public RecommendationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> RecommendAsync(object features)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5000/recommend", features);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RecommendationResponse>();
            return result?.Recommendation.FirstOrDefault();
        }

        public class RecommendationResponse
        {
            public List<string> Recommendation { get; set; }
        }
    }

}
