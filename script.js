// ===== GitFlex Application Logic =====

// DOM Elements
const deployForm = document.getElementById('deployForm');
const repoUrlInput = document.getElementById('repoUrl');
const loadingState = document.getElementById('loadingState');
const errorState = document.getElementById('errorState');
const errorMessage = document.getElementById('errorMessage');
const previewSection = document.getElementById('previewSection');
const previewFrame = document.getElementById('previewFrame');
const previewTitle = document.getElementById('previewTitle');
const previewRepo = document.getElementById('previewRepo');
const themeToggle = document.getElementById('themeToggle');
const successModal = document.getElementById('successModal');
const refreshBtn = document.getElementById('refreshBtn');
const openNewTabBtn = document.getElementById('openNewTabBtn');
const closePreviewBtn = document.getElementById('closePreviewBtn');
const exampleCards = document.querySelectorAll('.example-card');

// State
let currentRepoInfo = null;
let deployedUrl = null;

// Platform configurations
const platforms = {
    github: {
        api: 'https://api.github.com',
        raw: 'https://raw.githubusercontent.com',
        patterns: [
            /github\.com\/([^\/]+)\/([^\/]+)/i,
            /github\.com\/([^\/]+)\/([^\/]+)\/tree\/([^\/]+)/i
        ]
    },
    gitlab: {
        api: 'https://gitlab.com/api/v4',
        raw: 'https://gitlab.com',
        patterns: [
            /gitlab\.com\/([^\/]+)\/([^\/]+)/i
        ]
    },
    bitbucket: {
        api: 'https://api.bitbucket.org/2.0',
        raw: 'https://bitbucket.org',
        patterns: [
            /bitbucket\.org\/([^\/]+)\/([^\/]+)/i
        ]
    },
    gitea: {
        api: null,
        raw: null,
        patterns: [
            /gitea\.com\/([^\/]+)\/([^\/]+)/i
        ]
    }
};

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    initTheme();
    setupEventListeners();
    loadFromURL();
});

// Theme Management
function initTheme() {
    const savedTheme = localStorage.getItem('theme') || 'dark';
    document.documentElement.setAttribute('data-theme', savedTheme);
    updateThemeIcon(savedTheme);
}

function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateThemeIcon(newTheme);
}

function updateThemeIcon(theme) {
    const icon = themeToggle.querySelector('i');
    if (theme === 'dark') {
        icon.className = 'fas fa-moon';
    } else {
        icon.className = 'fas fa-sun';
    }
}

// Event Listeners
function setupEventListeners() {
    deployForm.addEventListener('submit', handleDeploy);
    themeToggle.addEventListener('click', toggleTheme);
    refreshBtn.addEventListener('click', refreshPreview);
    openNewTabBtn.addEventListener('click', openInNewTab);
    closePreviewBtn.addEventListener('click', closePreview);
    
    exampleCards.forEach(card => {
        card.addEventListener('click', () => {
            const url = card.getAttribute('data-url');
            repoUrlInput.value = url;
            handleDeploy({ preventDefault: () => {} });
        });
    });

    // Keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeModal();
            closeError();
        }
    });
}

// Deploy Handler
async function handleDeploy(e) {
    e.preventDefault();
    
    const url = repoUrlInput.value.trim();
    if (!url) {
        showError('Пожалуйста, введите URL репозитория');
        return;
    }

    // Show loading state
    showLoading();
    hideError();
    hidePreview();

    try {
        const repoInfo = parseRepoUrl(url);
        if (!repoInfo) {
            throw new Error('Не удалось распознать URL репозитория. Поддерживаются GitHub, GitLab, Bitbucket и другие платформы.');
        }

        currentRepoInfo = repoInfo;
        
        // Fetch repository information
        await fetchRepository(repoInfo);
        
        // Generate preview URL
        deployedUrl = generatePreviewUrl(repoInfo);
        
        // Update preview
        updatePreview(repoInfo);
        
        // Show success
        showSuccess();
        
    } catch (error) {
        console.error('Deploy error:', error);
        showError(error.message || 'Произошла ошибка при загрузке репозитория');
    } finally {
        hideLoading();
    }
}

// Parse Repository URL
function parseRepoUrl(url) {
    // Try to match against known platforms
    for (const [platform, config] of Object.entries(platforms)) {
        for (const pattern of config.patterns) {
            const match = url.match(pattern);
            if (match) {
                return {
                    platform,
                    owner: match[1],
                    repo: match[2].replace('.git', ''),
                    branch: match[3] || 'main',
                    fullUrl: url
                };
            }
        }
    }

    // Generic pattern for any Git platform
    const genericPattern = /(?:https?:\/\/)?(?:www\.)?([^\/]+\.[^\/]+)\/([^\/]+)\/([^\/]+)/i;
    const match = url.match(genericPattern);
    if (match) {
        return {
            platform: 'generic',
            host: match[1],
            owner: match[2],
            repo: match[3].replace('.git', ''),
            branch: 'main',
            fullUrl: url
        };
    }

    return null;
}

// Fetch Repository Information
async function fetchRepository(repoInfo) {
    const { platform, owner, repo, branch } = repoInfo;

    if (platform === 'github') {
        try {
            const response = await fetch(`${platforms.github.api}/repos/${owner}/${repo}`);
            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error('Репозиторий не найден. Проверьте URL и убедитесь, что он публичный.');
                } else if (response.status === 403) {
                    // Rate limit exceeded, continue without API data
                    console.warn('GitHub API rate limit exceeded');
                } else {
                    throw new Error(`Ошибка API GitHub: ${response.status}`);
                }
            } else {
                const data = await response.json();
                repoInfo.defaultBranch = data.default_branch || branch;
                repoInfo.description = data.description;
                repoInfo.stars = data.stargazers_count;
            }
        } catch (error) {
            // Continue without API data for private repos or rate limits
            repoInfo.defaultBranch = branch;
            console.warn('Could not fetch repo details:', error);
        }
    } else if (platform === 'gitlab') {
        try {
            const encodedPath = encodeURIComponent(`${owner}/${repo}`);
            const response = await fetch(`${platforms.gitlab.api}/projects/${encodedPath}`);
            if (response.ok) {
                const data = await response.json();
                repoInfo.defaultBranch = data.default_branch || branch;
                repoInfo.description = data.description;
            }
        } catch (error) {
            repoInfo.defaultBranch = branch;
            console.warn('Could not fetch GitLab repo details:', error);
        }
    } else {
        repoInfo.defaultBranch = branch;
    }
}

// Generate Preview URL
function generatePreviewUrl(repoInfo) {
    const { platform, owner, repo, defaultBranch } = repoInfo;

    switch (platform) {
        case 'github':
            return `${platforms.github.raw}/${owner}/${repo}/${defaultBranch}/`;
        case 'gitlab':
            return `${platforms.raw}/${owner}/${repo}/-/raw/${defaultBranch}/`;
        case 'bitbucket':
            return `${platforms.raw}/${owner}/${repo}/raw/${defaultBranch}/`;
        default:
            // For generic platforms, try common raw URL patterns
            if (repoInfo.host) {
                return `https://${repoInfo.host}/${owner}/${repo}/raw/${defaultBranch}/`;
            }
            return null;
    }
}

// Update Preview
function updatePreview(repoInfo) {
    const { platform, owner, repo, defaultBranch, description } = repoInfo;
    
    previewTitle.textContent = `${owner}/${repo}`;
    previewRepo.textContent = description || `${platform.toUpperCase()} • Branch: ${defaultBranch}`;
    
    // Create a blob URL with the content
    loadRepositoryContent(repoInfo);
}

// Load Repository Content
async function loadRepositoryContent(repoInfo) {
    const { platform, owner, repo, defaultBranch } = repoInfo;
    
    try {
        let files = [];
        
        if (platform === 'github') {
            files = await fetchGitHubFiles(owner, repo, defaultBranch);
        } else if (platform === 'gitlab') {
            files = await fetchGitLabFiles(owner, repo, defaultBranch);
        }
        
        // Find index file
        const indexFile = findIndexFile(files);
        
        if (indexFile) {
            const contentUrl = getFileUrl(repoInfo, indexFile);
            previewFrame.src = contentUrl;
            showPreview();
        } else {
            // No index file found, show file listing
            showFileListing(files, repoInfo);
        }
    } catch (error) {
        console.error('Error loading content:', error);
        // Try direct URL as fallback
        const directUrl = generatePreviewUrl(repoInfo);
        if (directUrl) {
            previewFrame.src = directUrl + 'index.html';
            showPreview();
        } else {
            throw new Error('Не удалось загрузить содержимое репозитория');
        }
    }
}

// Fetch GitHub Files
async function fetchGitHubFiles(owner, repo, branch) {
    try {
        const response = await fetch(`${platforms.github.api}/repos/${owner}/${repo}/git/trees/${branch}?recursive=1`);
        if (response.ok) {
            const data = await response.json();
            return data.tree.filter(item => item.type === 'blob');
        }
    } catch (error) {
        console.warn('Could not fetch file list:', error);
    }
    return [];
}

// Fetch GitLab Files
async function fetchGitLabFiles(owner, repo, branch) {
    try {
        const encodedPath = encodeURIComponent(`${owner}/${repo}`);
        const response = await fetch(`${platforms.gitlab.api}/projects/${encodedPath}/repository/tree?ref=${branch}&recursive=true`);
        if (response.ok) {
            const data = await response.json();
            return data.filter(item => item.type === 'blob');
        }
    } catch (error) {
        console.warn('Could not fetch GitLab file list:', error);
    }
    return [];
}

// Find Index File
function findIndexFile(files) {
    const priorities = [
        'index.html',
        'index.htm',
        'default.html',
        'main.html',
        'app.html'
    ];
    
    for (const priority of priorities) {
        const file = files.find(f => f.path.toLowerCase() === priority);
        if (file) return file;
    }
    
    // Fallback to first HTML file
    return files.find(f => f.path.toLowerCase().endsWith('.html'));
}

// Get File URL
function getFileUrl(repoInfo, file) {
    const { platform, owner, repo, defaultBranch } = repoInfo;
    const filePath = file.path;
    
    switch (platform) {
        case 'github':
            return `${platforms.github.raw}/${owner}/${repo}/${defaultBranch}/${filePath}`;
        case 'gitlab':
            return `${platforms.raw}/${owner}/${repo}/-/raw/${defaultBranch}/${filePath}`;
        case 'bitbucket':
            return `${platforms.raw}/${owner}/${repo}/raw/${defaultBranch}/${filePath}`;
        default:
            return generatePreviewUrl(repoInfo) + filePath;
    }
}

// Show File Listing
function showFileListing(files, repoInfo) {
    const htmlContent = `
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Файлы репозитория</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    padding: 2rem;
                }
                .container {
                    background: white;
                    border-radius: 20px;
                    padding: 3rem;
                    max-width: 800px;
                    width: 100%;
                    box-shadow: 0 20px 60px rgba(0,0,0,0.3);
                }
                h1 {
                    color: #1a1a2e;
                    margin-bottom: 0.5rem;
                    font-size: 2rem;
                }
                p {
                    color: #666;
                    margin-bottom: 2rem;
                }
                .file-list {
                    list-style: none;
                }
                .file-item {
                    padding: 1rem;
                    border: 1px solid #eee;
                    border-radius: 10px;
                    margin-bottom: 0.5rem;
                    transition: all 0.3s ease;
                    cursor: pointer;
                }
                .file-item:hover {
                    background: #f8f9fa;
                    border-color: #667eea;
                    transform: translateX(5px);
                }
                .file-icon {
                    margin-right: 0.5rem;
                    color: #667eea;
                }
            </style>
        </head>
        <body>
            <div class="container">
                <h1>📁 Файлы репозитория</h1>
                <p>index.html не найден. Выберите файл для просмотра:</p>
                <ul class="file-list">
                    ${files.slice(0, 20).map(file => `
                        <li class="file-item" onclick="window.location.href='${getFileUrl(repoInfo, file)}'">
                            <span class="file-icon">📄</span>
                            ${file.path}
                        </li>
                    `).join('')}
                </ul>
            </div>
        </body>
        </html>
    `;
    
    const blob = new Blob([htmlContent], { type: 'text/html' });
    const url = URL.createObjectURL(blob);
    previewFrame.src = url;
    showPreview();
}

// UI Functions
function showLoading() {
    loadingState.style.display = 'block';
    deployForm.style.display = 'none';
}

function hideLoading() {
    loadingState.style.display = 'none';
    deployForm.style.display = 'block';
}

function showError(message) {
    errorMessage.textContent = message;
    errorState.style.display = 'block';
}

function hideError() {
    errorState.style.display = 'none';
}

function showPreview() {
    previewSection.style.display = 'block';
    previewSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function hidePreview() {
    previewSection.style.display = 'none';
}

function showSuccess() {
    successModal.classList.add('active');
    setTimeout(() => {
        successModal.classList.remove('active');
    }, 3000);
}

function closeModal() {
    successModal.classList.remove('active');
}

function closeError() {
    hideError();
}

function closePreview() {
    hidePreview();
    previewFrame.src = 'about:blank';
    deployedUrl = null;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function refreshPreview() {
    if (currentRepoInfo) {
        showLoading();
        setTimeout(() => {
            loadRepositoryContent(currentRepoInfo);
            hideLoading();
        }, 500);
    }
}

function openInNewTab() {
    if (deployedUrl) {
        window.open(deployedUrl, '_blank');
    } else if (currentRepoInfo) {
        const url = generatePreviewUrl(currentRepoInfo);
        if (url) {
            window.open(url, '_blank');
        }
    }
}

// Load from URL parameters
function loadFromURL() {
    const params = new URLSearchParams(window.location.search);
    const repo = params.get('repo');
    if (repo) {
        repoUrlInput.value = decodeURIComponent(repo);
        handleDeploy({ preventDefault: () => {} });
    }
}

// Share functionality
function shareDeployment() {
    if (currentRepoInfo) {
        const url = `${window.location.origin}${window.location.pathname}?repo=${encodeURIComponent(currentRepoInfo.fullUrl)}`;
        navigator.clipboard.writeText(url).then(() => {
            alert('Ссылка скопирована в буфер обмена!');
        });
    }
}

// Console branding
console.log('%c GitFlex ', 'background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; font-size: 20px; font-weight: bold; padding: 10px 20px; border-radius: 5px;');
console.log('%c Deploy anything, anywhere 🚀 ', 'color: #667eea; font-size: 14px;');
console.log('%c Made with ❤️ by TOO Oink Tech Ltd Co ', 'color: #ec4899; font-size: 12px;');
