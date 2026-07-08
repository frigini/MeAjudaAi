using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Ratings;

[ExcludeFromCodeCoverage]
public class ReviewBuilder : BaseBuilder<Review>
{
    private Guid? _providerId;
    private Guid? _customerId;
    private int? _rating;
    private string? _comment;
    private EReviewStatus? _status;

    public ReviewBuilder()
    {
        Faker = new Faker<Review>()
            .CustomInstantiator(f =>
            {
                var review = Review.Create(
                    _providerId ?? f.Random.Guid(),
                    _customerId ?? f.Random.Guid(),
                    _rating ?? f.Random.Int(1, 5),
                    _comment ?? f.Lorem.Sentence()
                );

                if (_status == EReviewStatus.Approved)
                    review.Approve();
                else if (_status == EReviewStatus.Rejected)
                    review.Reject("Test rejection reason");
                else if (_status == EReviewStatus.Flagged)
                    review.MarkAsFlagged();

                return review;
            });
    }

    public ReviewBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    public ReviewBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public ReviewBuilder WithRating(int rating)
    {
        _rating = rating;
        return this;
    }

    public ReviewBuilder WithComment(string? comment)
    {
        _comment = comment;
        return this;
    }

    public ReviewBuilder WithStatus(EReviewStatus status)
    {
        _status = status;
        return this;
    }

    public ReviewBuilder AsPending()
    {
        _status = EReviewStatus.Pending;
        return this;
    }

    public ReviewBuilder AsApproved()
    {
        _status = EReviewStatus.Approved;
        return this;
    }

    public ReviewBuilder AsRejected()
    {
        _status = EReviewStatus.Rejected;
        return this;
    }

    public ReviewBuilder AsFlagged()
    {
        _status = EReviewStatus.Flagged;
        return this;
    }
}
