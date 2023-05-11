var menu = document.getElementById("menu");
document.getElementById("menu-button").addEventListener("click", function() {
  if (window.getComputedStyle(menu).display == "none") {
    menu.style.display = "block";
  } else {
    menu.style.display = "none";
  }
});


var mediaQuery = window.matchMedia("(min-width: 831px)")

function handleMediaQuery(event) {
  if (event.matches) {
    menu.style.display = "flex";
  } else {
    menu.style.display = "none";
  }
}

mediaQuery.addEventListener('change', handleMediaQuery);
handleMediaQuery(mediaQuery);

// I will be creating a different pen with touch support but right // now I have no time for it due to school
const NOMBRE_DE_ROLES = 8;

const slider = document.querySelector(".items");
		const slides = document.querySelectorAll(".item");
		const button = document.querySelectorAll(".button");

		let current = 0;
		let prev = NOMBRE_DE_ROLES-1;
		let next = 1;

		for (let i = 0; i < button.length; i++) {
			button[i].addEventListener("click", () => i == 0 ? gotoPrev() : gotoNext());
		}

		const gotoPrev = () => current > 0 ? gotoNum(current - 1) : gotoNum(slides.length - 1);

		const gotoNext = () => current < NOMBRE_DE_ROLES-1 ? gotoNum(current + 1) : gotoNum(0);

		const gotoNum = number => {
			current = number;
      
			prev = current - 1;
			next = current + 1;

			for (let i = 0; i < slides.length; i++) {
        // ---------------------------------------------------------------- //
        if (slides[i].classList.contains("active")) {
          var previousElem = slides[i].previousSibling;
          while (previousElem && previousElem.nodeType != 1) { // assurez-vous que l'élément précédent est un élément, pas un nœud texte
            previousElem = previousElem.previousSibling;
          }
          var nextDiv = slides[i].nextElementSibling;
          nextDiv.classList.add("descriptionCache");
          nextDiv.classList.remove("descriptionActive");
          if (previousElem) {
            previousElem.style.display = "none";
          }
        }
        // ---------------------------------------------------------------- //
				slides[i].classList.remove("active");
				slides[i].classList.remove("prev");
				slides[i].classList.remove("next");
			}

			if (next == NOMBRE_DE_ROLES) {
				next = 0;
			}

			if (prev == -1) {
				prev = NOMBRE_DE_ROLES-1;
			}

      // ---------------------------------------------------------------- //
      var previousElement = slides[current].previousSibling;
      while (previousElement && previousElement.nodeType != 1) { // assurez-vous que l'élément précédent est un élément, pas un nœud texte
          previousElement = previousElement.previousSibling;
      }
      if (previousElement) {
        previousElement.style.display = "block";
      } 

      var nextDivContaineur = slides[current].nextElementSibling;
      nextDivContaineur.classList.remove("descriptionCache");
      nextDivContaineur.classList.add("descriptionActive");
      // ---------------------------------------------------------------- //

			slides[current].classList.add("active");
			slides[prev].classList.add("prev");
			slides[next].classList.add("next");
		}