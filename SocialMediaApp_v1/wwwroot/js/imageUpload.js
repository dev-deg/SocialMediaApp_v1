// wwwroot/js/imageUpload.js
document.addEventListener('DOMContentLoaded', function() {
    const imageUpload = document.getElementById('imageUpload');
    const imagePreview = document.getElementById('imagePreview');
    const imagePreviewContainer = document.getElementById('imagePreviewContainer');
    const imageUrlInput = document.getElementById('imageUrl');
    const removeImageBtn = document.getElementById('removeImage');
    const uploadProgress = document.getElementById('uploadProgress');
    const progressBar = uploadProgress.querySelector('.progress-bar');
    const postForm = document.getElementById('postForm');
    
    // Show image preview when a file is selected
    imageUpload.addEventListener('change', function() {
        if (this.files && this.files[0]) {
            const file = this.files[0];
            
            // Validate file size (max 5MB)
            if (file.size > 5 * 1024 * 1024) {
                alert('File size exceeds 5MB. Please choose a smaller image.');
                this.value = '';
                return;
            }
            
            // Create preview
            const reader = new FileReader();
            reader.onload = function(e) {
                imagePreview.src = e.target.result;
                imagePreviewContainer.classList.remove('d-none');
            };
            reader.readAsDataURL(file);
            
            // Upload the image immediately
            uploadImage(file);
        }
    });
    
    // Remove selected image
    removeImageBtn.addEventListener('click', function() {
        imageUpload.value = '';
        imagePreviewContainer.classList.add('d-none');
        imageUrlInput.value = '';
    });
    
    // Upload the image to the server
    function uploadImage(file) {
        const formData = new FormData();
        formData.append('file', file);
        
        // Show progress bar
        uploadProgress.classList.remove('d-none');
        progressBar.style.width = '0%';
        
        const xhr = new XMLHttpRequest();
        
        // Track upload progress
        xhr.upload.addEventListener('progress', function(e) {
            if (e.lengthComputable) {
                const percentComplete = (e.loaded / e.total) * 100;
                progressBar.style.width = percentComplete + '%';
            }
        });
        
        xhr.onload = function() {
            if (xhr.status === 200) {
                const response = JSON.parse(xhr.responseText);
                if (response.success) {
                    imageUrlInput.value = response.imageUrl;
                    uploadProgress.classList.add('d-none');
                } else {
                    handleUploadError('Upload failed: ' + response.message);
                }
            } else {
                handleUploadError('Upload failed with status: ' + xhr.status);
            }
        };
        
        xhr.onerror = function() {
            handleUploadError('Network error occurred during upload');
        };
        
        xhr.open('POST', '/UploadImage', true);
        xhr.send(formData);
    }
    
    function handleUploadError(message) {
        console.error(message);
        alert(message);
        uploadProgress.classList.add('d-none');
        imagePreviewContainer.classList.add('d-none');
        imageUpload.value = '';
    }
    
    // Prevent form submission if image is still uploading
    postForm.addEventListener('submit', function(e) {
        if (!uploadProgress.classList.contains('d-none')) {
            e.preventDefault();
            alert('Please wait for the image to finish uploading');
        }
    });
});