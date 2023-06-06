﻿using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface IAuthorData
{
    Task<int> AddAuthorAsync(AuthorModel author);
    Task<int> DeleteAuthorAsync(AuthorModel author);
    Task<List<AuthorModel>> GetAuthorsAsync();
    Task<int> UpdateAuthorAsync(AuthorModel author);
}