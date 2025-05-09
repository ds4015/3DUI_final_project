import requests
import os
import time
import json
import logging
from datetime import datetime

# Set up logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class ArtInstituteAPI:
    def __init__(self):
        self.api_url = "https://api.artic.edu/api/v1"
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        }
        self.download_dir = "art_institute_drawings"
        os.makedirs(self.download_dir, exist_ok=True)

    def search_artworks(self, page=1, per_page=100):
        """Search for drawings and sketches using the API"""
        endpoint = f"{self.api_url}/artworks/search"
        params = {
            'page': page,
            'limit': per_page,
            'q': 'rough pencil sketch',
            'fields': 'id,title,image_id,artist_title,date_display,medium_display,thumbnail,api_link',
            'filter[term][is_public_domain]': 'true',
            'filter[term][artwork_type_id]': '4',
            'filter[match][medium_display]': 'pencil'
        }
        
        try:
            response = requests.get(endpoint, params=params, headers=self.headers)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            logger.error(f"Error searching artworks: {e}")
            if hasattr(e.response, 'text'):
                logger.error(f"Response text: {e.response.text}")
            return None

    def get_image_url(self, image_id):
        """Get the full resolution image URL via the AIC IIIF server"""
        if not image_id:
            return []
        base = "https://www.artic.edu/iiif/2"
        # 843px wide is the default “full” width in their examples
        return [f"{base}/{image_id}/full/843,/0/default.jpg"]


    def download_image(self, image_urls, filename):
        """Download an image and save it, trying different sizes if needed"""
        for url in image_urls:
            try:
                response = requests.get(url, headers=self.headers)
                if response.status_code == 200 and len(response.content) > 0:
                    # Check if the response is actually an image
                    content_type = response.headers.get('content-type', '')
                    if 'image' in content_type:
                        filepath = os.path.join(self.download_dir, filename)
                        with open(filepath, 'wb') as f:
                            f.write(response.content)
                        logger.info(f"Downloaded: {filename} from {url}")
                        return True
                    else:
                        logger.debug(f"Response is not an image for {url}: {content_type}")
                else:
                    logger.debug(f"Empty response or non-200 status for {url}: {response.status_code}")
            except requests.exceptions.RequestException as e:
                logger.debug(f"Failed to download {url}: {e}")
                continue
        
        logger.error(f"Failed to download {filename} from any available size")
        return False

    def save_metadata(self, artworks):
        """Save artwork metadata to a JSON file"""
        metadata_file = os.path.join(self.download_dir, "metadata.json")
        with open(metadata_file, 'w', encoding='utf-8') as f:
            json.dump(artworks, f, indent=2, ensure_ascii=False)
        logger.info(f"Saved metadata to {metadata_file}")

    def scrape(self, max_pages=10):
        """Main scraping function"""
        all_artworks = []
        page = 1
        successful_downloads = 0
        failed_downloads = 0

        while page <= max_pages:
            logger.info(f"Scraping page {page}")
            data = self.search_artworks(page=page)
            
            if not data or not data.get('data'):
                break

            artworks = data['data']
            all_artworks.extend(artworks)

            for artwork in artworks:
                if not artwork.get('image_id'):
                    logger.warning(f"No image ID for artwork {artwork.get('id')}")
                    continue

                # Create filename from artwork title and ID
                title = artwork.get('title', 'untitled').lower()
                title = ''.join(c if c.isalnum() else '_' for c in title)
                filename = f"{title}_{artwork['id']}.jpg"

                # Download image
                image_urls = self.get_image_url(artwork['image_id'])
                if image_urls:
                    if self.download_image(image_urls, filename):
                        successful_downloads += 1
                    else:
                        failed_downloads += 1

                # Be nice to the server
                time.sleep(0.5)

            page += 1
            time.sleep(1)

        # Save all metadata
        self.save_metadata(all_artworks)
        logger.info(f"Scraping completed. Successfully downloaded: {successful_downloads}, Failed: {failed_downloads}")

def main():
    api = ArtInstituteAPI()
    api.scrape(max_pages=10)  # Adjust max_pages as needed

if __name__ == "__main__":
    main() 