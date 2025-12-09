# Video Library Database Features

## Overview
The Raspberry Pi Video Player now includes a comprehensive video library management system using SQLite database for storing and organizing video files with rich metadata.

## Features

### 📹 Video File Management
- **Automatic Metadata Extraction**: Duration, resolution, codecs, file size
- **Rich Video Information**: Title, description, genre, play statistics
- **Favorite System**: Mark videos as favorites for easy access
- **Play Statistics**: Track play count and last played date
- **File Organization**: Smart categorization by genre and other metadata

### 🗄️ Database Storage
- **SQLite Database**: Local storage at `%LocalApplicationData%/PiVCR/pivcr.db`
- **Entity Framework Core**: Modern ORM with migrations support
- **Indexed Fields**: Optimized queries for title, genre, dates, favorites
- **Automatic Database Creation**: No manual setup required

## Menu Options

### 6. Video Library
Access the comprehensive video library management system with these sub-options:

#### 1. Browse All Videos
- View complete collection with metadata
- Sort alphabetically by title
- Display duration, file size, play count, and favorites

#### 2. Search Videos
- Search across title, description, and genre
- Case-insensitive text matching
- Instant results display

#### 3. Browse by Genre
- Automatic genre extraction and listing
- Category-based browsing
- Quick genre selection

#### 4. Recently Played
- Shows last 10 played videos
- Ordered by most recent playback
- Quick access to favorite content

#### 5. Most Played
- Top 10 most frequently played videos
- Play count statistics
- Popular content discovery

#### 6. Favorites
- Starred/favorite videos collection
- Quick access to preferred content
- Personal curation system

#### 7. Add Video to Library
- Manual video file addition
- Custom title and description input
- Genre categorization
- Automatic metadata analysis

#### 8. Scan Directory for Videos
- Bulk import from directories
- Recursive scanning option
- Automatic duplicate detection
- Progress reporting

## Video File Model

### Core Properties
- **Id**: Unique database identifier
- **Title**: Display name (customizable)
- **FilePath**: Absolute file system path
- **Description**: Optional detailed description
- **Genre**: Categorization field

### Technical Metadata
- **Duration**: Video length (TimeSpan)
- **FileSizeBytes**: File size in bytes
- **VideoCodec**: Video compression format
- **AudioCodec**: Audio compression format
- **Width/Height**: Video resolution
- **FrameRate**: Frames per second
- **BitRate**: Video bitrate

### Usage Statistics
- **DateAdded**: When added to library
- **LastPlayed**: Most recent playback time
- **PlayCount**: Number of times played
- **IsFavorite**: User preference flag

### Display Properties
- **DisplayName**: Smart title/filename display
- **FileSizeDisplay**: Human-readable file size (KB/MB/GB)
- **ResolutionDisplay**: Formatted resolution (e.g., "1920x1080")

## Database Integration

### Automatic Features
- **Play Tracking**: Every video playback updates statistics
- **Metadata Analysis**: FFMpeg integration for technical details
- **Duplicate Prevention**: File path uniqueness constraints
- **Error Recovery**: Graceful handling of missing files

### Performance Optimizations
- **Database Indexing**: Fast searches on common fields
- **Lazy Loading**: Efficient memory usage
- **Connection Pooling**: Optimized database access
- **Async Operations**: Non-blocking UI interactions

## Usage Examples

### Adding Videos
```
Select option: 6 (Video Library)
Select option: 7 (Add video to library)
Enter video file path: /home/pi/Videos/movie.mp4
Enter title: My Favorite Movie
Enter description: A great action movie
Enter genre: Action
```

### Scanning Directories
```
Select option: 6 (Video Library)
Select option: 8 (Scan directory for videos)
Enter directory path: /home/pi/Videos
Scan subdirectories recursively? y
Scanning /home/pi/Videos...
Scan completed. 25 new video(s) added to library.
```

### Browsing and Playing
```
Select option: 6 (Video Library)
Select option: 1 (Browse all videos)

All Videos:
------------------------------------------------------
  1. Action Movie              01:45:30   2.1 GB  ⭐ (5 plays)
  2. Comedy Special            00:58:15   1.3 GB     (2 plays)
  3. Documentary               02:15:45   3.2 GB  ⭐ (8 plays)

Total: 3 video(s)
Select video to play (1-3): 1
```

## RFID Integration

### Enhanced RFID Mapping
- **Database Lookup**: RFID tags can reference database video IDs
- **Automatic Statistics**: RFID-triggered playback tracks statistics
- **Metadata Display**: Rich video information for RFID selections

### Configuration Example
```json
{
  "1234567890": "/path/to/video.mp4",
  "0987654321": "db:video_id:15"
}
```

## File Management

### Supported Formats
- MP4, AVI, MKV, MOV, WMV
- FLV, WebM, M4V, 3GP
- Automatic format detection

### Storage Locations
- **Database**: `%LocalApplicationData%/PiVCR/pivcr.db`
- **Configuration**: `rfid-config.json` (RFID mappings)
- **Settings**: `appsettings.json` (application settings)

## Technical Architecture

### Class Structure
- **VideoFile Model**: Entity definition with validation
- **VideoPlayerDbContext**: Entity Framework database context
- **VideoFileService**: Business logic and data operations
- **VideoManager**: Updated with database integration

### Dependencies
- **Microsoft.EntityFrameworkCore.Sqlite**: Database provider
- **Microsoft.EntityFrameworkCore.Design**: Development tools
- **FFMpegCore**: Video analysis and metadata extraction

## Migration and Upgrades

### Automatic Migration
- Database schema updates handled automatically
- Backward compatibility maintained
- Data preservation during upgrades

### Manual Operations
- Export/import capabilities
- Database backup and restore
- Performance optimization tools

## Best Practices

### Organization
- Use descriptive titles for easy browsing
- Categorize videos with genres
- Regular library maintenance and cleanup

### Performance
- Regular database maintenance
- Periodic cleanup of orphaned records
- Monitor storage space usage

### Backup
- Regular database backups recommended
- Export configurations before major changes
- Test restore procedures periodically
