using System;

namespace Umbraco.Cms.Core.Models.PublishedContent
{
    /// <inheritdoc />
    /// <summary>
    /// Indicates that the class is a published content model for a specified content type.
    /// </summary>
    /// <remarks>By default, the name of the class is assumed to be the content type alias. The
    /// <c>PublishedContentModelAttribute</c> can be used to indicate a different alias.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PublishedModelAttribute : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedModelAttribute" /> class with a content type alias.
        /// </summary>
        /// <param name="contentTypeAlias">The content type alias.</param>
        public PublishedModelAttribute(string contentTypeAlias)
        {
            if (contentTypeAlias == null) throw new ArgumentNullException(nameof(contentTypeAlias));
            if (string.IsNullOrWhiteSpace(contentTypeAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));

            ContentTypeAlias = contentTypeAlias;
        }


        public PublishedModelAttribute(string contentTypeAlias, string contentTypeName, bool allowedAsRoot, string contentTypeDescription = null, string icon = null, string thumbnail = null)
        {
            if (contentTypeAlias == null)
                throw new ArgumentNullException(nameof(contentTypeAlias));
            if (string.IsNullOrWhiteSpace(contentTypeAlias))
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));

            if (contentTypeName == null)
                throw new ArgumentNullException(nameof(contentTypeName));
            if (string.IsNullOrWhiteSpace(contentTypeName))
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));

            ContentTypeAlias = contentTypeAlias;
            ContentTypeName = contentTypeName;
            ContentTypeDescription = contentTypeDescription;
            ContentTypeIcon = icon;
            ContentTypeThumbnail = thumbnail;
            AllowedAsRoot = allowedAsRoot;
        }

        /// <summary>
        /// Gets or sets the content type alias.
        /// </summary>
        public string ContentTypeAlias { get; }

        /// <summary>
        /// Get or sets the Name
        /// </summary>
        public string ContentTypeName { get; }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string ContentTypeDescription { get; }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        /// <remarks>e.g. icon-document</remarks>
        public string ContentTypeIcon { get; }

        /// <summary>
        /// Gets or sets the Thumbnail
        /// </summary>
        /// <remarks>e.g. folder.png</remarks>
        public string ContentTypeThumbnail { get; }

        /// <summary>
        /// Gets or sets the Allowed As Root flag
        /// </summary>
        public bool AllowedAsRoot { get; }
    }
}
