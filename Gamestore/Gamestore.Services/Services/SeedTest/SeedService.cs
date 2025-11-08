using Bogus;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.SeedTest;

public class SeedService(IUnitOfWork unitOfWork, ILogger<SeedService> logger)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<SeedService> _logger = logger;

    public async Task SeedGamesAsync(int count = 100000)
    {
        try
        {
            var existingCount = await _unitOfWork.Games.CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("Database already has {Count} games, skipping seed", existingCount);
                return;
            }

            _logger.LogInformation("Starting seed of {Count} games...", count);

            // Seed Publishers
            var publisherFaker = new Faker<Publisher>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.CompanyName, f => f.Company.CompanyName())
                .RuleFor(p => p.Description, f => f.Lorem.Sentence());

            var publishers = publisherFaker.Generate(100);
            foreach (var publisher in publishers)
            {
                await _unitOfWork.Publishers.AddAsync(publisher);
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Seeded {Count} publishers", publishers.Count);

            // Seed Genres
            var genreFaker = new Faker<Genre>()
                .RuleFor(g => g.Id, _ => Guid.NewGuid())
                .RuleFor(g => g.Name, f => f.Lorem.Word());

            var genres = genreFaker.Generate(50);
            foreach (var genre in genres)
            {
                await _unitOfWork.Genres.AddAsync(genre);
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Seeded {Count} genres", genres.Count);

            // Seed Platforms
            var platformFaker = new Faker<Platform>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.Type, f => f.Lorem.Word());

            var platforms = platformFaker.Generate(20);
            foreach (var platform in platforms)
            {
                await _unitOfWork.Platforms.AddAsync(platform);
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Seeded {Count} platforms", platforms.Count);

            // Seed Games in batches
            var gameFaker = new Faker<Game>()
                .RuleFor(g => g.Id, _ => Guid.NewGuid())
                .RuleFor(g => g.Key, (f, g) => Guid.NewGuid().ToString()[..8])
                .RuleFor(g => g.Name, f => f.Lorem.Sentence(3))
                .RuleFor(g => g.Description, f => f.Lorem.Paragraph())
                .RuleFor(g => g.Price, f => f.Random.Double(9.99, 79.99))
                .RuleFor(g => g.UnitInStock, f => f.Random.Int(0, 1000))
                .RuleFor(g => g.Discontinued, f => f.Random.Int(0, 5))
                .RuleFor(g => g.PublisherId, _ => publishers[new Random().Next(publishers.Count)].Id)
                .RuleFor(g => g.ViewCount, f => f.Random.Int(0, 10000));

            const int batchSize = 1000;
            for (int i = 0; i < count; i += batchSize)
            {
                var batchCount = Math.Min(batchSize, count - i);
                var games = gameFaker.Generate(batchCount);

                foreach (var game in games)
                {
                    await _unitOfWork.Games.AddAsync(game);

                    // Add random genres - one by one
                    var addedGenres = new HashSet<Guid>();
                    var genreCount = new Random().Next(1, Math.Min(4, genres.Count));
                    for (int j = 0; j < genreCount; j++)
                    {
                        var randomGenre = genres[new Random().Next(genres.Count)];

                        // Skip if already added this genre
                        if (addedGenres.Contains(randomGenre.Id))
                        {
                            continue;
                        }

                        addedGenres.Add(randomGenre.Id);
                        await _unitOfWork.GameGenres.AddAsync(new GameGenre
                        {
                            GameId = game.Id,
                            GenreId = randomGenre.Id,
                        });
                    }

                    // Add random platforms - one by one
                    var addedPlatforms = new HashSet<Guid>();
                    var platformCount = new Random().Next(1, Math.Min(3, platforms.Count));
                    for (int j = 0; j < platformCount; j++)
                    {
                        var randomPlatform = platforms[new Random().Next(platforms.Count)];

                        // Skip if already added this platform
                        if (addedPlatforms.Contains(randomPlatform.Id))
                        {
                            continue;
                        }

                        addedPlatforms.Add(randomPlatform.Id);
                        await _unitOfWork.GamePlatforms.AddAsync(new GamePlatform
                        {
                            GameId = game.Id,
                            PlatformId = randomPlatform.Id,
                        });
                    }
                }

                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Seeded {Current}/{Total} games", Math.Min(i + batchSize, count), count);
            }

            _logger.LogInformation("✅ Seed completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during seeding");
            throw;
        }
    }
}