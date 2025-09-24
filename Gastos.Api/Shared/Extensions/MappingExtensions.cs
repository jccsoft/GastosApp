namespace Gastos.Api.Shared.Extensions;

public static class MappingExtensions
{
    #region ToDTO
    public static ApiPagedResponse<TDto> ToDto<T, TDto>(this ApiPagedResponse<T> pagedResponse, Func<T, TDto> toDtoFunc)
    {
        return new ApiPagedResponse<TDto>
        {
            UserId = pagedResponse.UserId,
            Items = [.. pagedResponse.Items.Select(toDtoFunc)],
            TotalItems = pagedResponse.TotalItems,
            Page = pagedResponse.Page,
            PageSize = pagedResponse.PageSize
        };
    }

    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            UserId = product.UserId,
            Name = product.Name,
            ImageUrl = product.ImageUrl,
            UnitsPack = product.UnitsPack,
            SizingId = product.SizingId,
            SizingValue = product.SizingValue,
            Sizing = product.Sizing?.ToDto()
        };
    }

    public static StoreDto ToDto(this Store store)
    {
        return new StoreDto
        {
            Id = store.Id,
            UserId = store.UserId,
            Name = store.Name,
            SourceName = store.SourceName
        };
    }

    public static ReceiptDto ToDto(this Receipt receipt)
    {
        return new ReceiptDto
        {
            Id = receipt.Id,
            UserId = receipt.UserId,
            SourceId = receipt.SourceId,
            TransactionDateUtc = receipt.TransactionDateUtc,
            StoreId = receipt.StoreId,
            Store = receipt.Store?.ToDto(),
            Items = [.. receipt.Items.Select(i => i.ToDto())]
        };
    }

    public static ReceiptItemDto ToDto(this ReceiptItem item)
    {
        return new ReceiptItemDto
        {
            Id = item.Id,
            ReceiptId = item.ReceiptId,
            ProductId = item.ProductId,
            Product = item.Product?.ToDto(),
            SourceDescription = item.SourceDescription,
            Quantity = item.Quantity,
            Amount = item.Amount
        };
    }

    public static SizingDto ToDto(this Sizing sizing)
    {
        return new SizingDto
        {
            Id = sizing.Id,
            Name = sizing.Name
        };
    }

    public static StatDto ToDto(this Stat stat)
    {
        return new StatDto
        {
            Date = stat.Date,
            Amount = stat.Amount
        };
    }
    #endregion



    #region ToEntity
    public static Product ToEntity(this ProductDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Name = dto.Name,
            ImageUrl = dto.ImageUrl,
            UnitsPack = dto.UnitsPack,
            SizingId = dto.SizingId,
            SizingValue = dto.SizingValue,
            Sizing = dto.Sizing?.ToEntity(),
            ReceiptItems = null! // avoid circular reference
        };
    }

    public static Store ToEntity(this StoreDto dto)
    {
        return new Store
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Name = dto.Name,
            SourceName = dto.SourceName,
            Receipts = null! // avoid circular reference
        };
    }

    public static Receipt ToEntity(this ReceiptDto dto)
    {
        return new Receipt
        {
            Id = dto.Id,
            UserId = dto.UserId,
            SourceId = dto.SourceId,
            TransactionDateUtc = dto.TransactionDateUtc,
            StoreId = dto.StoreId,
            Store = dto.Store?.ToEntity(),
            Items = [.. dto.Items.Select(i => i.ToEntity())]
        };
    }

    public static ReceiptItem ToEntity(this ReceiptItemDto dto)
    {
        return new ReceiptItem
        {
            Id = dto.Id,
            ReceiptId = dto.ReceiptId,
            Receipt = null!, // avoid circular reference
            ProductId = dto.ProductId,
            Product = dto.Product?.ToEntity(),
            SourceDescription = dto.SourceDescription,
            Quantity = dto.Quantity,
            Amount = dto.Amount
        };
    }

    public static Sizing ToEntity(this SizingDto dto)
    {
        return new Sizing
        {
            Id = dto.Id,
            Name = dto.Name,
            Products = null! // avoid circular reference
        };
    }

    public static Stat ToEntity(this StatDto dto)
    {
        return new Stat
        {
            Date = dto.Date,
            Amount = dto.Amount
        };
    }
    #endregion

}