import requests
import json
import os
from PIL import Image
import shutil

def get_chapter_titles(url):
    try:
        # Make a request to the API endpoint
        response = requests.get(url, verify=False)

        # Check if the request was successful
        if response.status_code == 200:
            # Parse the JSON response
            data = response.json()

            # Extract chapter titles
            chapters = data.get("data", [])

            return chapters
        else:
            print(f"Error: API request failed with status code {response.status_code}")
            return []

    except requests.RequestException as e:
        print(f"Error connecting to API: {e}")
        return []
    except json.JSONDecodeError as e:
        print(f"Error parsing JSON response: {e}")
        return []

def get_images(folder_path):
    images = []
    for root, dirs, files in os.walk(folder_path):
        for file in files:
            if file.lower().endswith(( '.jpg', '.jpeg')):
                images.append(os.path.join(root, file))
    return images

def main():
    url = 'https://localhost:7110/api/v1/book/a6b88eac-89d0-419d-9739-eea0ebf28579/chapters'
    titles = get_chapter_titles(url)

    print("Chapter Titles:")
    for index, chapter in enumerate(titles, 1):
        print(f"{index}. {chapter}")

    folder_path = r'I:\Source\封面\西游记'
    output_dir = r'I:\Source\封面\西游记\output'
    images = get_images(folder_path)
    for index, image_path in enumerate(images, 1):
        print(f"{index}. {image_path}")

    for index, chapter in enumerate(titles, 1):
        if index <= len(images):
            image_path = images[index - 1]
            try:
                print(f"Processing chapter '{chapter}' with image '{image_path}'")
                # os.rename(image_path, os.path.join(output_dir, f"{chapter}.jpg"))
                shutil.copy(image_path, os.path.join(output_dir, f"{chapter}.jpg"))

            except Exception as e:
                print(f"Failed to save image for chapter '{chapter}': {e}")
        else:
            print(f"No image available for chapter '{chapter}'")


if __name__ == "__main__":
    main()